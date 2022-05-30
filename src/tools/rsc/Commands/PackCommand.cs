namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class PackCommand : CancellableAsyncCommand<PackCommand.PackCommandSettings>
{
    public sealed class PackCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input directory")]
        public string Input { get; }

        [CommandArgument(1, "<output>")]
        [Description("Output file")]
        public string Output { get; }

        [CommandOption("--encryption-key <key>")]
        [Description("Set encryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> EncryptionKey { get; init; } = ResourceContainer.LatestKey;

        public PackCommandSettings(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando, PackCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.WriteLine($"Packing [cyan]{settings.Input}[/] to [cyan]{settings.Output}[/]...");

        var files = await progress.RunTaskAsync(
            "Gather resource files",
            () => Task.FromResult(
                new DirectoryInfo(settings.Input)
                    .EnumerateFiles()
                    .OrderBy(f => f.FullName, StringComparer.Ordinal)
                    .ToArray()));

        var rc = ResourceContainer.Create();

        await progress.RunTaskAsync(
            "Load resource files",
            files.Length,
            increment => Parallel.ForEachAsync(
                files,
                cancellationToken,
                async (file, cancellationToken) =>
                {
                    ResourceContainerEntry entry;

                    lock (rc)
                        entry = rc.CreateEntry(file.Name);

                    entry.Data = await File.ReadAllBytesAsync(file.FullName, cancellationToken);

                    increment();
                }));

        await progress.RunTaskAsync(
            "Save resource container",
            async () =>
            {
                await using var stream = File.Open(settings.Output, FileMode.Create, FileAccess.Write);

                await rc.SaveAsync(
                    stream,
                    new ResourceContainerSaveOptions()
                        .WithKey(settings.EncryptionKey.Span),
                    cancellationToken);
            });

        return 0;
    }
}
