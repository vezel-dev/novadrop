using Vezel.Novadrop.Helpers;

namespace Vezel.Novadrop.Commands;

sealed class PackCommand : Command
{
    public PackCommand()
        : base("pack", "Pack the contents of a directory to a data center file.")
    {
        var inputArg = new Argument<DirectoryInfo>(
            "input",
            "Input directory");
        var outputArg = new Argument<FileInfo>(
            "output",
            "Output file");
        var compressionOpt = new Option<CompressionLevel>(
            "--compression",
            () => CompressionLevel.Optimal,
            "Set compression level");

        Add(inputArg);
        Add(outputArg);
        Add(compressionOpt);

        this.SetHandler(
            async (
                InvocationContext context,
                DirectoryInfo input,
                FileInfo output,
                CompressionLevel compression,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Packing '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                var dc = DataCenter.Create();
                var files = input.GetFiles("?*-?*.xml", SearchOption.AllDirectories);

                using (var handler = new DataSheetValidationHandler(context))
                    await Parallel.ForEachAsync(
                        files,
                        cancellationToken,
                        async (file, cancellationToken) =>
                            await DataSheetLoader.LoadAsync(file, handler, dc.Root, cancellationToken));

                await using var stream = File.Open(output.FullName, FileMode.Create, FileAccess.Write);

                await dc.SaveAsync(
                    stream,
                    new DataCenterSaveOptions()
                        .WithCompressionLevel(compression),
                    cancellationToken);

                sw.Stop();

                Console.WriteLine($"Packed {files.Length} data sheets in {sw.Elapsed}.");
            },
            inputArg,
            outputArg,
            compressionOpt);
    }
}
