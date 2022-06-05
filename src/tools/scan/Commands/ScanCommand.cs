using Vezel.Novadrop.Scanners;

namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class ScanCommand : CancellableAsyncCommand<ScanCommand.ScanCommandSettings>
{
    public sealed class ScanCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<output>")]
        [Description("Output directory")]
        public string Output { get; }

        [CommandArgument(1, "[pid]")]
        [Description("TERA process ID")]
        public int ProcessId { get; init; } = -1;

        public ScanCommandSettings(string output)
        {
            Output = output;
        }
    }

    static readonly ReadOnlyMemory<GameScanner> _scanners = new GameScanner[]
    {
        new ClientVersionScanner(),
        new DataCenterScanner(),
        new ResourceContainerScanner(),
        new SystemMessageScanner(),
    };

    protected override async Task<int> ExecuteAsync(
        dynamic expando, ScanCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        using var proc = settings.ProcessId is not -1 and var pid
            ? Process.GetProcessById(pid)
            : Process.GetProcessesByName("TERA").FirstOrDefault();

        if (proc == null)
        {
            Log.WriteLine($"Could not find the TERA process.");

            return 1;
        }

        if (proc.MainModule?.ModuleName != "TERA.exe")
        {
            Log.WriteLine($"Process [cyan]{proc.Id}[/] does not look like TERA.");

            return 1;
        }

        Log.WriteLine($"Scanning TERA process [cyan]{proc.Id}[/] and writing results to [cyan]{settings.Output}[/]...");

        NativeAddress teraExeBase;
        byte[] teraExeImage;

        // Copy the executable into our local address space to speed up pattern searches considerably.
        using (var tera = new NativeProcess(proc.Id))
        {
            var teraExe = tera.MainModule.Window;

            teraExeBase = teraExe.Address;
            teraExeImage = new byte[teraExe.Length];

            teraExe.Read(0, teraExeImage);
        }

        var output = new DirectoryInfo(settings.Output);

        output.Create();

        var context = new ScanContext(
            new(
                new RebasingMemoryAccessor(new ManagedMemoryAccessor(teraExeImage), teraExeBase),
                teraExeBase,
                (nuint)teraExeImage.Length),
            output);
        var failed = new List<string>();
        var good = true;

        foreach (var scanner in _scanners.ToArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = scanner.GetType().Name;
            var result = await progress.RunTaskAsync($"Run {name}", () => scanner.RunAsync(context, cancellationToken));

            if (!result)
                failed.Add(name);

            good &= result;
        }

        foreach (var name in failed)
            Log.WriteLine($"[blue]{name}[/] failed to retrieve information from TERA process [cyan]{proc.Id}[/].");

        return good ? 0 : 1;
    }
}
