using Vezel.Novadrop.Scanners;

namespace Vezel.Novadrop;

static class Program
{
    static readonly ReadOnlyMemory<IScanner> _scanners = new IScanner[]
    {
        new ClientVersionScanner(),
        new DataCenterScanner(),
        new ResourceContainerScanner(),
        new SystemMessageScanner(),
    };

    static Task<int> Main(string[] args)
    {
        var outputArg = new Argument<DirectoryInfo>(
            "output",
            "Output directory");
        var pidArg = new Argument<int?>(
            "pid",
            "TERA process ID");
        var cmd = new RootCommand("Extract useful data from a running TERA client.")
        {
            outputArg,
            pidArg,
        };

        // TODO: https://github.com/dotnet/command-line-api/issues/1669
        pidArg.SetDefaultValue(-1);

        cmd.SetHandler(
            async (
                DirectoryInfo output,
                int? pid,
                CancellationToken cancellationToken) =>
            {
                var proc = pid is int p and not -1
                    ? Process.GetProcessById(p)
                    : Process.GetProcessesByName("TERA").FirstOrDefault();

                if (proc == null)
                    throw new ApplicationException("Could not find the TERA process.");

                if (proc.MainModule?.ModuleName != "TERA.exe")
                    throw new ApplicationException($"Process {proc.Id} does not look like TERA.");

                Console.WriteLine($"Attaching to TERA process {proc.Id}...");

                var sw1 = Stopwatch.StartNew();

                using var native = new NativeProcess(proc);

                output.Create();

                Console.WriteLine();

                var context = new ScanContext(native, output);
                var exceptions = new List<ApplicationException>();

                foreach (var scanner in _scanners.ToArray())
                {
                    var name = scanner.GetType().Name;

                    Console.WriteLine($"Running {name}...");

                    var sw2 = Stopwatch.StartNew();

                    try
                    {
                        await scanner.RunAsync(context);
                    }
                    catch (ApplicationException ex)
                    {
                        exceptions.Add(ex);
                    }

                    sw2.Stop();

                    Console.WriteLine($"Finished running {name} in {sw2.Elapsed}...");
                    Console.WriteLine();
                }

                sw1.Stop();

                Console.WriteLine($"Wrote results to directory '{output}' in {sw1.Elapsed}.");

                if (exceptions.Count != 0)
                    throw new AggregateException(null, exceptions);
            },
            outputArg,
            pidArg);

        return cmd.InvokeAsync(args);
    }
}
