using Vezel.Novadrop.Helpers;

namespace Vezel.Novadrop.Commands;

sealed class WatchCommand : Command
{
    public WatchCommand()
        : base("watch", "Watch the contents of a directory for changes and pack them to a data center file.")
    {
        var inputArg = new Argument<DirectoryInfo>(
            "input",
            "Input directory");
        var outputArg = new Argument<FileInfo>(
            "output",
            "Output file");
        var compressionOpt = new Option<CompressionLevel>(
            "--compression",
            () => CompressionLevel.Fastest,
            "Set compression level");

        Add(inputArg);
        Add(outputArg);
        Add(compressionOpt);

        this.SetHandler(
            async (
                DirectoryInfo input,
                FileInfo output,
                CompressionLevel compression,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Watching for changes in '{input}' and packing to '{output}'.");

                using var watcher = new DataSheetWatcher(input);

                var dc = DataCenter.Create();
                var root = dc.Root;
                var sheets = new Dictionary<string, DataCenterNode>();

                while (true)
                {
                    await Task.Delay(1000, cancellationToken);

                    var changes = sheets.ToDictionary(kvp => kvp.Key, _ => (DataSheetState?)DataSheetState.Unchanged);

                    // Coalesce multiple changes to the same file. We handle all possible state transitions since
                    // FileSystemWatcher is known to be a bit quirky (e.g. duplicate events).
                    while (watcher.TryDequeue(out var change))
                        changes[change.Path] = (changes.GetValueOrDefault(change.Path), change.State) switch
                        {
                            (null, DataSheetState.Created) => DataSheetState.Created,
                            (null, DataSheetState.Modified) => DataSheetState.Created,
                            (null, DataSheetState.Deleted) => null,

                            (DataSheetState.Unchanged, DataSheetState.Created) => DataSheetState.Unchanged,
                            (DataSheetState.Unchanged, DataSheetState.Modified) => DataSheetState.Modified,
                            (DataSheetState.Unchanged, DataSheetState.Deleted) => DataSheetState.Deleted,

                            (DataSheetState.Created, DataSheetState.Created) => DataSheetState.Created,
                            (DataSheetState.Created, DataSheetState.Modified) => DataSheetState.Created,
                            (DataSheetState.Created, DataSheetState.Deleted) => null,

                            (DataSheetState.Modified, DataSheetState.Created) => DataSheetState.Modified,
                            (DataSheetState.Modified, DataSheetState.Modified) => DataSheetState.Modified,
                            (DataSheetState.Modified, DataSheetState.Deleted) => DataSheetState.Deleted,

                            (DataSheetState.Deleted, DataSheetState.Created) => DataSheetState.Modified,
                            (DataSheetState.Deleted, DataSheetState.Modified) => DataSheetState.Modified,
                            (DataSheetState.Deleted, DataSheetState.Deleted) => DataSheetState.Deleted,

                            _ => throw new UnreachableException(),
                        };

                    var finalChanges = changes.Where(
                        kvp => kvp.Value is not (null or DataSheetState.Unchanged)).ToArray();

                    if (finalChanges.Length == 0)
                        continue;

                    Console.WriteLine();
                    Console.WriteLine($"{finalChanges.Length} data sheet change(s) detected:");

                    var shownChanges = finalChanges.Take(10).ToArray();

                    foreach (var (path, state) in shownChanges)
                    {
                        var (msg, color) = state switch
                        {
                            DataSheetState.Created => ('+', ConsoleColor.Green),
                            DataSheetState.Modified => ('*', ConsoleColor.Yellow),
                            DataSheetState.Deleted => ('-', ConsoleColor.Yellow),
                            var s => throw new InvalidDataException(s.ToString()),
                        };

                        Console.ForegroundColor = color;
                        Console.WriteLine($"  {msg} {path}");
                        Console.ResetColor();
                    }

                    var remainingChanges = finalChanges.Length - shownChanges.Length;

                    if (remainingChanges != 0)
                        Console.WriteLine($"  ... {remainingChanges} more change(s) ...");

                    Console.WriteLine();
                    Console.WriteLine("Applying data sheet change(s) to in-memory tree...");

                    var sw1 = Stopwatch.StartNew();

                    using (var handler = new DataSheetValidationHandler(null))
                        await Parallel.ForEachAsync(
                            finalChanges,
                            cancellationToken,
                            async (tup, cancellationToken) =>
                            {
                                var path = tup.Key;

                                // We assume that if loading a sheet results in errors, it must be modified again to
                                // become successfully loadable. So we just discard the current change information.

                                switch (tup.Value)
                                {
                                    case DataSheetState.Created:
                                    {
                                        var node = await DataSheetLoader.LoadAsync(
                                            new(path), handler, root, cancellationToken);

                                        if (node != null)
                                            lock (root)
                                                sheets.Add(path, node);

                                        break;
                                    }

                                    case DataSheetState.Modified:
                                    {
                                        var node = await DataSheetLoader.LoadAsync(
                                            new(path), handler, root, cancellationToken);

                                        lock (root)
                                        {
                                            if (node != null)
                                            {
                                                var old = sheets[path];

                                                _ = root.RemoveChild(old);

                                                sheets[path] = node;
                                            }
                                        }

                                        break;
                                    }

                                    case DataSheetState.Deleted:
                                    {
                                        lock (root)
                                        {
                                            var node = sheets[path];

                                            _ = root.RemoveChild(node);
                                            _ = sheets.Remove(path);
                                        }

                                        break;
                                    }
                                }
                            });

                    sw1.Stop();

                    Console.WriteLine($"Data sheet changes applied in {sw1.Elapsed}.");
                    Console.WriteLine($"Packing to '{output}'...");

                    var sw2 = Stopwatch.StartNew();

                    await using var stream = File.Open(output.FullName, FileMode.Create, FileAccess.Write);

                    // TODO: What if this fails?
                    await dc.SaveAsync(
                        stream,
                        new DataCenterSaveOptions()
                            .WithCompressionLevel(compression),
                        cancellationToken);

                    sw2.Stop();

                    Console.WriteLine($"Packed {dc.Root.Children.Count} data sheets in {sw2.Elapsed}.");
                }
            },
            inputArg,
            outputArg,
            compressionOpt);
    }
}
