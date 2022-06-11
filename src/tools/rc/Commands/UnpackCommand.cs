namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class UnpackCommand : CancellableAsyncCommand<UnpackCommand.UnpackCommandSettings>
{
    public sealed class UnpackCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input file")]
        public string Input { get; }

        [CommandArgument(1, "<output>")]
        [Description("Output directory")]
        public string Output { get; }

        [CommandOption("--decryption-key <key>")]
        [Description("Set decryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> DecryptionKey { get; init; } = ResourceContainer.LatestKey;

        [CommandOption("--strict")]
        [Description("Enable strict verification")]
        public bool Strict { get; init; }

        public UnpackCommandSettings(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando, UnpackCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.WriteLine($"Unpacking [cyan]{settings.Input}[/] to [cyan]{settings.Output}[/]...");

        var rc = await progress.RunTaskAsync(
            "Load resource container",
            async () =>
            {
                await using var inStream = File.OpenRead(settings.Input);

                return await ResourceContainer.LoadAsync(
                    inStream,
                    new ResourceContainerLoadOptions()
                        .WithKey(settings.DecryptionKey.Span)
                        .WithStrict(settings.Strict),
                    cancellationToken);
            });

        var entries = rc.Entries;

        await progress.RunTaskAsync(
            "Write resource files",
            entries.Count,
            async increment =>
            {
                _ = Directory.CreateDirectory(settings.Output);

                await Parallel.ForEachAsync(
                    rc.Entries,
                    cancellationToken,
                    async (kvp, cancellationToken) =>
                    {
                        await using var stream = File.Open(
                            Path.Combine(settings.Output, kvp.Key), FileMode.Create, FileAccess.Write);

                        await stream.WriteAsync(kvp.Value.Data, cancellationToken);

                        increment();
                    });
            });

        return 0;
    }
}
