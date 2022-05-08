namespace Vezel.Novadrop.Commands;

sealed class UnpackCommand : Command
{
    public UnpackCommand()
        : base("unpack", "Unpack the contents of a data center file to a directory.")
    {
        var inputArg = new Argument<FileInfo>("input", "Input file");
        var outputArg = new Argument<DirectoryInfo>("output", "Output directory");
        var strictOpt = new Option<bool>("--strict", () => false, "Enable strict verification");

        Add(inputArg);
        Add(outputArg);
        Add(strictOpt);

        this.SetHandler(
            async (FileInfo input, DirectoryInfo output, bool strict, CancellationToken cancellationToken) =>
            {
                // TODO: Implement.
                await Task.Yield();
                await Task.Yield();
            },
            inputArg,
            outputArg,
            strictOpt);
    }
}
