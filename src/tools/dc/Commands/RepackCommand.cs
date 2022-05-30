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
        public ReadOnlyMemory<byte> DecryptionKey { get; init; } = DataCenter.LatestKey;

        [CommandOption("--decryption-iv <iv>")]
        [Description("Set decryption IV")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> DecryptionIV { get; init; } = DataCenter.LatestIV;

        [CommandOption("--strict")]
        [Description("Enable strict verification")]
        public bool Strict { get; init; }

        [CommandOption("--compression <level>")]
        [Description("Set compression level")]
        public CompressionLevel Compression { get; init; } = CompressionLevel.Optimal;

        [CommandOption("--encryption-key <key>")]
        [Description("Set encryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> EncryptionKey { get; init; } = DataCenter.LatestKey;

        [CommandOption("--encryption-iv <iv>")]
        [Description("Set encryption IV")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> EncryptionIV { get; init; } = DataCenter.LatestIV;

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

        var root = await progress.RunTaskAsync(
            "Load data center",
            async () =>
            {
                await using var inStream = File.OpenRead(settings.Input);

                return await DataCenter.LoadAsync(
                    inStream,
                    new DataCenterLoadOptions()
                        .WithKey(settings.DecryptionKey.Span)
                        .WithIV(settings.DecryptionIV.Span)
                        .WithStrict(settings.Strict)
                        .WithLoaderMode(DataCenterLoaderMode.Eager)
                        .WithMutability(DataCenterMutability.Immutable),
                    cancellationToken);
            });

        await progress.RunTaskAsync(
            "Save data center",
            async () =>
            {
                await using var stream = File.Open(settings.Output, FileMode.Create, FileAccess.Write);

                await DataCenter.SaveAsync(
                    root,
                    stream,
                    new DataCenterSaveOptions()
                        .WithCompressionLevel(settings.Compression)
                        .WithKey(settings.EncryptionKey.Span)
                        .WithIV(settings.EncryptionIV.Span),
                    cancellationToken);
            });

        return 0;
    }
}
