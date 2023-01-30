namespace Vezel.Novadrop.Client;

public sealed partial class LauncherProcess : GameProcess
{
    // Represents a Tl.exe process from the perspective of a launcher.exe-compatible process.

    public event Action? ServerListUriRequested;

    public event Action? AuthenticationInfoRequested;

    public event ReadOnlySpanAction<string, int>? WebUriRequested;

    public event Action<GameEvent>? GameEventOccurred;

    public event Action<int>? GameExited;

    public LauncherProcessOptions Options { get; }

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    public LauncherProcess(LauncherProcessOptions options)
    {
        Check.Null(options);

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
        nuint id, scoped ReadOnlySpan<byte> payload)
    {
        var utf8 = Encoding.UTF8;

        string? HandleGameEventOrExit(string value)
        {
            // csPopup(), endPopup(%d), gameEvent(%d), promoPopup(%d)

            if (GameEventRegex().Match(value) is { Success: true } m1)
                GameEventOccurred?.Invoke((GameEvent)int.Parse(m1.Groups[1].Value, _culture));
            else if (EndPopupRegex().Match(value) is { Success: true } m2)
                GameExited?.Invoke((int)uint.Parse(m2.Groups[1].Value, _culture));

            return null;
        }

        string HandleServerListUriRequest()
        {
            ServerListUriRequested?.Invoke();

            return Options.ServerListUri.AbsoluteUri + '\0';
        }

        string HandleAuthenticationInfoRequest()
        {
            AuthenticationInfoRequested?.Invoke();

            return JsonSerializer.Serialize(
                new LauncherAuthenticationInfo(
                    Options.AccountName,
                    Options.SessionTicket,
                    Options.Servers.Values.Select(
                        s => new LauncherAuthenticationInfo.ServerCharacters(s.Id, s.Characters)),
                    Options.LastServerId),
                LauncherJsonContext.Default.LauncherAuthenticationInfo) + '\0';
        }

        string HandleWebUriRequest(Match match)
        {
            var id = int.Parse(match.Groups[1].Value, _culture);
            var args = match.Groups[2].Value.Split(',');

            WebUriRequested?.Invoke(args, id);

            if (Options.WebUriProvider?.Invoke(id, args) is not Uri uri)
                return string.Empty;

            Check.Operation(uri.IsAbsoluteUri);

            return uri.AbsoluteUri;
        }

        // Note that the message ID increments on every sent message for slsurl, gamestr, ticket, last_svr, and
        // char_cnt. So, for these messages, matching on the contents is the only correct way to handle them.
        var replyPayload = (id, utf8.GetString(payload)) switch
        {
            (0x0dbadb0a, "Hello!!\0") => "Hello!!\0",
            (0x0, var value) => HandleGameEventOrExit(value),
            (_, "slsurl\0") => HandleServerListUriRequest(),
            (_, "gamestr\0" or "ticket\0" or "last_svr\0" or "char_cnt\0") => HandleAuthenticationInfoRequest(),
            (_, var value) when GetWebLinkUrlRegex().Match(value) is { Success: true } m => HandleWebUriRequest(m),
            _ => null,
        };

        return replyPayload != null ? (id, utf8.GetBytes(replyPayload)) : null;
    }

    [GeneratedRegex("^gameEvent\\((\\d+)\\)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex GameEventRegex();

    [GeneratedRegex("^endPopup\\((\\d+)\\)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex EndPopupRegex();

    [GeneratedRegex("^getWebLinkUrl\\((\\d+),(.*)\\)$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex GetWebLinkUrlRegex();
}
