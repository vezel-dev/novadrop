namespace Vezel.Novadrop.Client;

public sealed class LauncherProcess : GameProcess
{
    // Represents a Tl.exe process from the perspective of a launcher.exe-compatible process.

    public event Action? ServerListUriRequested;

    public event Action? AuthenticationInfoRequested;

    public event Action<GameEvent>? GameEventOccurred;

    public event Action<int>? GameExited;

    public LauncherProcessOptions Options { get; }

    static readonly Regex _gameEvent = new(@"^gameEvent\((\d+)\)$");

    static readonly Regex _endPopup = new(@"^endPopup\((\d+)\)$");

    static readonly Regex _getWebLinkUrl = new(@"^getWebLinkUrl\((\d+),(.*)\)$");

    public LauncherProcess(LauncherProcessOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    protected override void GetWindowConfiguration(out string className, out string windowName)
    {
        className = windowName = "EME.LauncherWnd";
    }

    protected override void GetProcessConfiguration(out string fileName, out string[] arguments)
    {
        fileName = Options.FileName;
        arguments = Array.Empty<string>();
    }

    protected override (nuint Id, ReadOnlyMemory<byte> Payload)? HandleWindowMessage(
        nuint id, ReadOnlySpan<byte> payload)
    {
        var opts = Options;
        var utf8 = Encoding.UTF8;

        string? HandleGameEventOrExit(string value)
        {
            // csPopup(), endPopup(%d), gameEvent(%d), promoPopup(%d)

            if (_gameEvent.Match(value) is { Success: true } m1)
                GameEventOccurred?.Invoke((GameEvent)int.Parse(m1.Groups[1].ValueSpan));
            else if (_endPopup.Match(value) is { Success: true } m2)
                GameExited?.Invoke((int)uint.Parse(m2.Groups[1].ValueSpan));

            return null;
        }

        string HandleServerListUriRequest()
        {
            ServerListUriRequested?.Invoke();

            return opts.ServerListUri.AbsoluteUri;
        }

        string HandleAuthenticationInfoRequest()
        {
            AuthenticationInfoRequested?.Invoke();

            return JsonSerializer.Serialize(
                new LauncherAuthenticationInfo(
                    opts.AccountName,
                    opts.SessionTicket,
                    opts.Servers.Values
                        .Select(s => new LauncherAuthenticationInfo.ServerCharacters(s.Id, s.Characters)),
                    opts.LastServerId),
                LauncherJsonContext.Default.LauncherAuthenticationInfo);
        }

        string HandleWebUriRequest()
        {
            return string.Empty;
        }

        // Note that the message ID increments on every sent message for slsurl, gamestr, ticket, last_svr, and
        // char_cnt. So, for these messages, matching on the contents is the only correct way to handle them.
        var replyPayload = (id, utf8.GetString(payload)) switch
        {
            (0x0dbadb0a, "Hello!!\0") => "Hello!!",
            (0x0, var value) => HandleGameEventOrExit(value),
            (_, "slsurl\0") => HandleServerListUriRequest(),
            (_, "gamestr\0" or "ticket\0" or "last_svr\0" or "char_cnt\0") => HandleAuthenticationInfoRequest(),
            (_, var value) when _getWebLinkUrl.IsMatch(value) => HandleWebUriRequest(),
            _ => null,
        };

        return replyPayload != null ? (id, utf8.GetBytes(replyPayload + '\0')) : null; // Add NUL terminator.
    }
}
