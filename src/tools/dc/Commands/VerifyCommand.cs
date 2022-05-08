namespace Vezel.Novadrop.Commands;

sealed class VerifyCommand : Command
{
    [SuppressMessage("", "CA5350")]
    public VerifyCommand()
        : base("verify", "Verify the format integrity of a data center file.")
    {
        var inputArg = new Argument<FileInfo>("input", "Input file");
        var strictOpt = new Option<bool>("--strict", () => true, "Enable strict verification");

        Add(inputArg);
        Add(strictOpt);

        this.SetHandler(
            async (FileInfo input, bool strict, CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Verifying {input}...");

                await using var stream = input.OpenRead();

                void PrintHash(string name, HashAlgorithm algorithm)
                {
                    var hash = algorithm.ComputeHash(stream);
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (var b in hash)
                        _ = sb.Append(CultureInfo.InvariantCulture, $"{b:x2}");

                    Console.WriteLine($"{name}: {sb}");

                    stream.Position = 0;
                }

                using var sha1 = SHA1.Create();
                using var sha256 = SHA256.Create();
                using var sha384 = SHA384.Create();
                using var sha512 = SHA512.Create();

                PrintHash("SHA-1", sha1);
                PrintHash("SHA-256", sha256);
                PrintHash("SHA-384", sha384);
                PrintHash("SHA-512", sha512);

                var sw = Stopwatch.StartNew();

                var dc = await DataCenter.LoadAsync(
                    stream,
                    new DataCenterLoadOptions().WithStrict(strict),
                    cancellationToken);

                var nodes = 0;
                var attrs = 0;

                void ForceLoad(DataCenterNode node)
                {
                    nodes++;
                    attrs += node.Attributes.Count;

                    foreach (var child in node.Children)
                        ForceLoad(child);
                }

                ForceLoad(dc.Root);

                sw.Stop();

                Console.WriteLine($"Verified {nodes} nodes and {attrs} attributes in {sw.Elapsed}.");
            },
            inputArg,
            strictOpt);
    }
}
