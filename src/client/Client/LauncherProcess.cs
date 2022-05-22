namespace Vezel.Novadrop.Client;

public sealed class LauncherProcess : GameProcess
{
    // Represents a Tl.exe process from the perspective of a launcher.exe-compatible process.

    public LauncherProcessOptions Options { get; }

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
        var replyPayload = id switch
        {
            0x0dbadb0a => "Hello!!",
            0 => null,
            2 => opts.ServerListUri.AbsoluteUri,
            3 => JsonSerializer.Serialize(
                new LauncherGameInfo(opts.AccountName, Convert.ToHexString(opts.Ticket.Span), opts.LastServerId),
                LauncherJsonContext.Default.LauncherGameInfo),
            4 => null,
            5 => null,
            6 => null,
            7 => null,
            8 => null,
            10 => null,
            _ => null,
        };

        return replyPayload != null ? (id, Encoding.UTF8.GetBytes(replyPayload + '\0')) : null;
    }
}
