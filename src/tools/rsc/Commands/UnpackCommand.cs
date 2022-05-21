namespace Vezel.Novadrop.Commands;

sealed class UnpackCommand : Command
{
    public UnpackCommand()
        : base("unpack", "Unpack the contents of a resource container file to a directory.")
    {
        var inputArg = new Argument<FileInfo>(
            "input",
            "Input file")
            .ExistingOnly();
        var outputArg = new Argument<DirectoryInfo>(
            "output",
            "Output directory")
            .LegalFilePathsOnly();
        var decryptionKeyOpt = new HexStringOption(
            "--decryption-key",
            ResourceContainer.LatestKey,
            "Decryption key");
        var strictOpt = new Option<bool>(
            "--strict",
            () => false,
            "Enable strict verification");

        Add(inputArg);
        Add(outputArg);
        Add(decryptionKeyOpt);
        Add(strictOpt);

        this.SetHandler(
            async (
                FileInfo input,
                DirectoryInfo output,
                ReadOnlyMemory<byte> decryptionKey,
                bool strict,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Unpacking '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                await using var stream = input.OpenRead();

                var rc = await ResourceContainer.LoadAsync(
                    stream,
                    new ResourceContainerLoadOptions()
                        .WithKey(decryptionKey.Span)
                        .WithStrict(strict),
                    cancellationToken);

                output.Create();

                await Parallel.ForEachAsync(
                    rc.Entries,
                    cancellationToken,
                    async (kvp, cancellationToken) =>
                    {
                        await using var stream = File.Open(
                            Path.Combine(output.FullName, kvp.Key), FileMode.Create, FileAccess.Write);

                        await stream.WriteAsync(kvp.Value.Data, cancellationToken);
                    });

                sw.Stop();

                Console.WriteLine($"Unpacked {rc.Entries.Count} entries in {sw.Elapsed}.");
            },
            inputArg,
            outputArg,
            decryptionKeyOpt,
            strictOpt);
    }
}
