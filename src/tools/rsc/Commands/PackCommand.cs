namespace Vezel.Novadrop.Commands;

sealed class PackCommand : Command
{
    public PackCommand()
        : base("pack", "Pack the contents of a directory to a resource container file.")
    {
        var inputArg = new Argument<DirectoryInfo>(
            "input",
            "Input directory");
        var outputArg = new Argument<FileInfo>(
            "output",
            "Output file");

        Add(inputArg);
        Add(outputArg);

        this.SetHandler(
            async (
                InvocationContext context,
                DirectoryInfo input,
                FileInfo output,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Packing '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                var rc = ResourceContainer.Create();
                var files = input.GetFiles();

                await Parallel.ForEachAsync(
                    files,
                    cancellationToken,
                    async (file, cancellationToken) =>
                    {
                        ResourceContainerEntry entry;

                        lock (rc)
                            entry = rc.CreateEntry(file.Name);

                        entry.Data = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
                    });

                await using var stream = File.Open(output.FullName, FileMode.Create, FileAccess.Write);

                await rc.SaveAsync(stream, new ResourceContainerSaveOptions(), cancellationToken);

                sw.Stop();

                Console.WriteLine($"Packed {files.Length} entries in {sw.Elapsed}.");
            },
            inputArg,
            outputArg);
    }
}
