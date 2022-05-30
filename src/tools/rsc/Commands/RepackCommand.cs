namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class RepackCommand : CancellableAsyncCommand<RepackCommand.RepackCommandSettings>
{
    public sealed class RepackCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input file")]
        public string Input { get; }

        [CommandArgument(1, "<output>")]
        [Description("Output file")]
        public string Output { get; }

        [CommandOption("--decryption-key <key>")]
        [Description("Set decryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> DecryptionKey { get; init; } = ResourceContainer.LatestKey;

        [CommandOption("--strict")]
        [Description("Enable strict verification")]
        public bool Strict { get; init; }

        [CommandOption("--encryption-key <key>")]
        [Description("Set encryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> EncryptionKey { get; init; } = ResourceContainer.LatestKey;

        public RepackCommandSettings(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando, RepackCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.WriteLine($"Repacking [cyan]{settings.Input}[/] to [cyan]{settings.Output}[/]...");

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
