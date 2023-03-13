namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class VerifyCommand : CancellableAsyncCommand<VerifyCommand.VerifyCommandSettings>
{
    public sealed class VerifyCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input file")]
        public string Input { get; }

        [CommandOption("--decryption-key <key>")]
        [Description("Set decryption key")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> DecryptionKey { get; init; } = ResourceContainer.LatestKey;

        [CommandOption("--strict")]
        [Description("Enable strict verification")]
        public bool Strict { get; init; }

        public VerifyCommandSettings(string input)
        {
            Input = input;
        }
    }

    [SuppressMessage("", "CA5350")]
    protected override async Task<int> ExecuteAsync(
        dynamic expando, VerifyCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.MarkupLineInterpolated($"Verifying [cyan]{settings.Input}[/]...");

        await progress.RunTaskAsync(
            "Compute resource container hashes",
            async () =>
            {
                await using var stream = File.OpenRead(settings.Input);

                [SuppressMessage("", "CA1308")]
                string ComputeHash(HashAlgorithm algorithm)
                {
                    return Convert.ToHexString(algorithm.ComputeHash(stream)).ToLowerInvariant();
                }

                using var sha1 = SHA1.Create();
                using var sha256 = SHA256.Create();
                using var sha384 = SHA384.Create();
                using var sha512 = SHA512.Create();

                expando.SHA1 = ComputeHash(sha1);
                expando.SHA256 = ComputeHash(sha256);
                expando.SHA384 = ComputeHash(sha384);
                expando.SHA512 = ComputeHash(sha512);
            });

        await progress.RunTaskAsync(
            "Load resource container",
            async () =>
            {
                await using var stream = File.OpenRead(settings.Input);

                var rc = await ResourceContainer.LoadAsync(
                    stream,
                    new ResourceContainerLoadOptions()
                        .WithKey(settings.DecryptionKey.Span)
                        .WithStrict(settings.Strict),
                    cancellationToken);

                expando.Entries = rc.Entries.Count;
            });

        return 0;
    }

    protected override Task PostExecuteAsync(
        dynamic expando, VerifyCommandSettings settings, CancellationToken cancellationToken)
    {
        Log.MarkupLineInterpolated($"SHA-1: [blue]{expando.SHA1}[/]");
        Log.MarkupLineInterpolated($"SHA-256: [blue]{expando.SHA256}[/]");
        Log.MarkupLineInterpolated($"SHA-384: [blue]{expando.SHA384}[/]");
        Log.MarkupLineInterpolated($"SHA-512: [blue]{expando.SHA512}[/]");
        Log.WriteLine();
        Log.MarkupLineInterpolated($"Entries: [blue]{expando.Entries}[/]");

        return Task.CompletedTask;
    }
}
