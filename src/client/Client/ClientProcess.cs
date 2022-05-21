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

    protected override string? HandleWindowMessage(nuint id, ReadOnlySpan<byte> payload)
    {
        // TODO: Handle window messages.
        return null;
    }
}
