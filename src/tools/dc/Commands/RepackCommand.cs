namespace Vezel.Novadrop.Commands;

sealed class RepackCommand : Command
{
    public RepackCommand()
        : base("repack", "Repack the contents of a data center file.")
    {
        var inputArg = new Argument<FileInfo>("input", "Input file");
        var outputArg = new Argument<FileInfo>("output", "Output file");
        var strictOpt = new Option<bool>("--strict", () => false, "Enable strict verification");
        var levelOpt = new Option<CompressionLevel>(
            "--compression",
            () => CompressionLevel.Fastest,
            "Set compression level");

        Add(inputArg);
        Add(outputArg);
        Add(strictOpt);
        Add(levelOpt);

        this.SetHandler(
            async (
                FileInfo input,
                FileInfo output,
                bool strict,
                CompressionLevel level,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Repacking '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                await using var inStream = input.OpenRead();

                var dc = await DataCenter.LoadAsync(
                    inStream,
                    new DataCenterLoadOptions()
                        .WithLoaderMode(DataCenterLoaderMode.Eager)
                        .WithStrict(strict)
                        .WithMutability(DataCenterMutability.Immutable),
                    cancellationToken);

                await using var outStream = output.Open(FileMode.Create, FileAccess.Write);

                await dc.SaveAsync(
                    outStream,
                    new DataCenterSaveOptions().WithCompressionLevel(level),
                    cancellationToken);

                sw.Stop();

                Console.WriteLine($"Repacked {dc.Root.Children.Count} data sheets in {sw.Elapsed}.");
            },
            inputArg,
            outputArg,
            strictOpt,
            levelOpt);
    }
}
