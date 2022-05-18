namespace Vezel.Novadrop.Commands;

sealed class RepackCommand : Command
{
    public RepackCommand()
        : base("repack", "Repack the contents of a resource container file.")
    {
        var inputArg = new Argument<FileInfo>(
            "input",
            "Input file");
        var outputArg = new Argument<FileInfo>(
            "output",
            "Output file");
        var strictOpt = new Option<bool>(
            "--strict",
            () => false,
            "Enable strict verification");

        Add(inputArg);
        Add(outputArg);
        Add(strictOpt);

        this.SetHandler(
            async (
                FileInfo input,
                FileInfo output,
                bool strict,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Repacking '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                await using var inStream = input.OpenRead();

                var rc = await ResourceContainer.LoadAsync(
                    inStream,
                    new ResourceContainerLoadOptions()
                        .WithStrict(strict),
                    cancellationToken);

                await using var outStream = output.Open(FileMode.Create, FileAccess.Write);

                await rc.SaveAsync(outStream, new ResourceContainerSaveOptions(), cancellationToken);

                sw.Stop();

                Console.WriteLine($"Repacked {rc.Entries.Count} entries in {sw.Elapsed}.");
            },
            inputArg,
            outputArg,
            strictOpt);
    }
}
