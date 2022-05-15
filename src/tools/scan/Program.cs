using Vezel.Novadrop.Scanners;

namespace Vezel.Novadrop;

static class Program
{
    static readonly ReadOnlyMemory<IScanner> _scanners = new IScanner[]
    {
        new ClientVersionScanner(),
        new DataCenterScanner(),
        new SystemMessageScanner(),
    };

    static int Main(string[] args)
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
            (DirectoryInfo output, int? pid) =>
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

                var native = new NativeProcess(proc);

                output.Create();

                var context = new ScanContext(native, output);
                var exceptions = new List<ApplicationException>();

                foreach (var scanner in _scanners.Span)
                {
                    var name = scanner.GetType().Name;

                    Console.WriteLine($"Running {name}...");

                    var sw2 = Stopwatch.StartNew();

                    try
                    {
                        scanner.Run(context);
                    }
                    catch (ApplicationException ex)
                    {
                        exceptions.Add(ex);
                    }

                    sw2.Stop();

                    Console.WriteLine($"Finished running {name} in {sw2.Elapsed}...");
                }

                sw1.Stop();

                Console.WriteLine($"Wrote results to directory '{output}' in {sw1.Elapsed}.");

                if (exceptions.Count != 0)
                    throw new AggregateException(null, exceptions);
            },
            outputArg,
            pidArg);

        return cmd.Invoke(args);
    }
}
