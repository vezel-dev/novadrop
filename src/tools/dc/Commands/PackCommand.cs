namespace Vezel.Novadrop.Commands;

sealed class PackCommand : Command
{
    public PackCommand()
        : base("pack", "Pack the contents of a directory to a data center file.")
    {
        var inputArg = new Argument<DirectoryInfo>("input", "Input directory");
        var outputArg = new Argument<FileInfo>("output", "Output file");
        var levelOpt = new Option<CompressionLevel>(
            "--compression",
            () => CompressionLevel.Fastest,
            "Set compression level");

        Add(inputArg);
        Add(outputArg);
        Add(levelOpt);

        this.SetHandler(
            async (DirectoryInfo input, FileInfo output, CompressionLevel level, CancellationToken cancellationToken) =>
            {
                // TODO: Implement.
                await Task.Yield();
                await Task.Yield();
            },
            inputArg,
            outputArg,
            levelOpt);
    }
}
