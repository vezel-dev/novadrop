namespace Vezel.Novadrop.Client;

public sealed class ClientProcess : GameProcess
{
    // Represents a TERA.exe process from the perspective of a Tl.exe-compatible process.

    public ClientProcessOptions Options { get; }

    public ClientProcess(ClientProcessOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    protected override void GetWindowConfiguration(out string className, out string windowName)
    {
        className = "LAUNCHER_CLASS";
        windowName = "LAUNCHER_WINDOW";
    }

    protected override void GetProcessConfiguration(out string fileName, out string[] arguments)
    {
        fileName = Options.FileName;
        arguments = Options.Language is string lang ? new[] { $"-LANGUAGEEXT={lang}" } : Array.Empty<string>();
    }

    protected override (nuint Id, ReadOnlyMemory<byte> Payload)? HandleWindowMessage(
        nuint id, ReadOnlySpan<byte> payload)
    {
        var opts = Options;

        return id switch
        {
            1 => (2, Encoding.Unicode.GetBytes(opts.AccountName)),
            3 => (4, opts.Ticket),
            5 => null, // TODO: Server list encoded with Protocol Buffers (id = 6).
            7 => null,
            26 => null,
            1000 => null,
            1001 => null,
            1002 => null,
            1003 => null,
            1004 => null,
            1011 => null,
            1012 => null,
            1020 => null,
            _ => null,
        };
    }
}
