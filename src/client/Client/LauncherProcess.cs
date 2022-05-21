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

    protected override string? HandleWindowMessage(nuint id, ReadOnlySpan<byte> payload)
    {
        var opts = Options;

        return id switch
        {
            0x0dbadb0a => "Hello!!",
            2 => opts.ServerListUri.AbsoluteUri,
            3 => JsonSerializer.Serialize(
                new LauncherGameInfo(opts.AccountName, opts.Ticket, opts.LastServerId),
                LauncherJsonContext.Default.LauncherGameInfo),
            _ => null,
        };
    }
}
