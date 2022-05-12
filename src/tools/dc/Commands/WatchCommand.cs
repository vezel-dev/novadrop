namespace Vezel.Novadrop.Commands;

sealed class WatchCommand : Command
{
    enum ChangeState
    {
        Unchanged,
        Created,
        Modified,
        Deleted,
    }

    sealed class DataSheetWatcher : IDisposable
    {
        const string SheetFilter = "?*-?*.xml";

        readonly ConcurrentQueue<(string, ChangeState)> _queue = new();

        readonly DirectoryInfo _directory;

        FileSystemWatcher _fsw;

        public DataSheetWatcher(DirectoryInfo directory)
        {
            _directory = directory;
            _fsw = CreateWatcher(true);
        }

        ~DataSheetWatcher()
        {
            Dispose();
        }

        public void Dispose()
        {
            _fsw?.Dispose();
            GC.SuppressFinalize(this);
        }

        FileSystemWatcher CreateWatcher(bool initial)
        {
            var fsw = new FileSystemWatcher(_directory.FullName)
            {
                Filter = SheetFilter,
                IncludeSubdirectories = true,
                NotifyFilter =
                    NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                InternalBufferSize = ushort.MaxValue,
            };

            fsw.Error += HandleError;
            fsw.Created += HandleCreated;
            fsw.Changed += HandleChanged;
            fsw.Deleted += HandleDeleted;
            fsw.Renamed += HandleRenamed;

            fsw.EnableRaisingEvents = true;

            if (initial)
                foreach (var file in _directory.EnumerateFiles(SheetFilter, SearchOption.AllDirectories))
                    Enqueue(file.FullName, ChangeState.Created);

            return fsw;
        }

        void Enqueue(string path, ChangeState state)
        {
            _queue.Enqueue((path, state));
        }

        void HandleCreated(object sender, FileSystemEventArgs e)
        {
            Enqueue(e.FullPath, ChangeState.Created);
        }

        void HandleChanged(object sender, FileSystemEventArgs e)
        {
            Enqueue(e.FullPath, ChangeState.Modified);
        }

        void HandleDeleted(object sender, FileSystemEventArgs e)
        {
            Enqueue(e.FullPath, ChangeState.Deleted);
        }

        void HandleRenamed(object sender, RenamedEventArgs e)
        {
            Enqueue(e.OldFullPath, ChangeState.Deleted);
            Enqueue(e.FullPath, ChangeState.Created);
        }

        void HandleError(object sender, ErrorEventArgs e)
        {
            if (e.GetException() is not Win32Exception)
            {
                _fsw.EnableRaisingEvents = false;

                _fsw.Renamed -= HandleRenamed;
                _fsw.Deleted -= HandleDeleted;
                _fsw.Changed -= HandleChanged;
                _fsw.Created -= HandleCreated;
                _fsw.Error -= HandleError;

                _fsw.Dispose();

                _fsw = CreateWatcher(false);
            }
        }

        public bool TryDequeue(out (string Path, ChangeState State) change)
        {
            return _queue.TryDequeue(out change);
        }
    }

    public WatchCommand()
        : base("watch", "Watch the contents of a directory for changes and pack them to a data center file.")
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
                Console.WriteLine($"Watching for changes in '{input}' and packing to '{output}'...");
                Console.WriteLine();

                using var watcher = new DataSheetWatcher(input);

                var dc = DataCenter.Create();
                var root = dc.Root;
                var sheets = new Dictionary<string, DataCenterNode>();

                var xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";

                while (true)
                {
                    await Task.Delay(1000, cancellationToken);

                    var changes = sheets.ToDictionary(kvp => kvp.Key, _ => (ChangeState?)ChangeState.Unchanged);

                    // Coalesce potential multiple changes to the same file. We handle all possible state transitions
                    // since FileSystemWatcher is known to be a bit quirky (e.g. duplicate events).
                    while (watcher.TryDequeue(out var change))
                        changes[change.Path] = (changes.GetValueOrDefault(change.Path), change.State) switch
                        {
                            (null, ChangeState.Created) => ChangeState.Created,
                            (null, ChangeState.Modified) => ChangeState.Created,
                            (null, ChangeState.Deleted) => null,

                            (ChangeState.Unchanged, ChangeState.Created) => ChangeState.Unchanged,
                            (ChangeState.Unchanged, ChangeState.Modified) => ChangeState.Modified,
                            (ChangeState.Unchanged, ChangeState.Deleted) => ChangeState.Deleted,

                            (ChangeState.Created, ChangeState.Created) => ChangeState.Created,
                            (ChangeState.Created, ChangeState.Modified) => ChangeState.Created,
                            (ChangeState.Created, ChangeState.Deleted) => null,

                            (ChangeState.Modified, ChangeState.Created) => ChangeState.Modified,
                            (ChangeState.Modified, ChangeState.Modified) => ChangeState.Modified,
                            (ChangeState.Modified, ChangeState.Deleted) => ChangeState.Deleted,

                            (ChangeState.Deleted, ChangeState.Created) => ChangeState.Modified,
                            (ChangeState.Deleted, ChangeState.Modified) => ChangeState.Modified,
                            (ChangeState.Deleted, ChangeState.Deleted) => ChangeState.Deleted,

                            _ => throw new InvalidOperationException(), // Impossible.
                        };

                    var finalChanges = changes.Where(kvp => kvp.Value is not null or ChangeState.Unchanged).ToArray();

                    if (finalChanges.Length == 0)
                        continue;

                    Console.WriteLine($"{finalChanges.Length} data sheet changes detected:");

                    var shownChanges = finalChanges.Take(25).ToArray();

                    foreach (var (path, state) in shownChanges)
                    {
                        var (msg, color) = state switch
                        {
                            ChangeState.Created => ('+', ConsoleColor.Green),
                            ChangeState.Modified => ('*', ConsoleColor.Yellow),
                            ChangeState.Deleted => ('-', ConsoleColor.Yellow),
                            _ => throw new InvalidOperationException(), // Impossible.
                        };

                        Console.ForegroundColor = color;
                        Console.WriteLine($"  {msg} {path}");
                        Console.ResetColor();
                    }

                    var remainingChanges = finalChanges.Length - shownChanges.Length;

                    if (remainingChanges != 0)
                        Console.WriteLine($"  ... {remainingChanges} more changes ...");

                    Console.WriteLine();
                    Console.WriteLine("Applying data sheet changes to in-memory tree...");

                    var sw1 = Stopwatch.StartNew();

                    var problems = new List<(string Name, FileInfo File, ValidationEventArgs Args)>();

                    async ValueTask<(bool, DataCenterNode)> LoadSheetAsync(FileInfo file)
                    {
                        var settings = new XmlReaderSettings
                        {
                            Async = true,
                        };

                        using var reader = XmlReader.Create(file.FullName, settings);

                        var doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);

                        // We need to access type and key info from the schema during tree construction, so we do the
                        // validation manually as we go rather than relying on validation support in XmlReader or
                        // XDocument. (Notably, the latter also has a very broken implementation that does not respect
                        // xsi:schemaLocation, even with an XmlUrlResolver set...)
                        var validator = new XmlSchemaValidator(
                            reader.NameTable,
                            settings.Schemas,
                            new XmlNamespaceManager(reader.NameTable),
                            XmlSchemaValidationFlags.ProcessSchemaLocation |
                            XmlSchemaValidationFlags.ReportValidationWarnings)
                        {
                            XmlResolver = new XmlUrlResolver(),
                            SourceUri = new Uri(reader.BaseURI),
                        };

                        var good = true;

                        validator.ValidationEventHandler += (_, e) =>
                        {
                            lock (problems)
                                problems.Add((file.Directory!.Name, file, e));

                            good = false;
                        };

                        validator.Initialize();

                        DataCenterValue AttributeToValue(XAttribute attribute)
                        {
                            var name = attribute.Name;
                            var value = validator.ValidateAttribute(
                                name.LocalName, name.NamespaceName, attribute.Value, null)!;

                            return value switch
                            {
                                int i => i,
                                float f => f,
                                string s => s,
                                bool b => b,
                                _ => 42, // Dummy value in case of validation failure.
                            };
                        }

                        var keyCache = new Dictionary<(string?, string?, string?, string?), DataCenterKeys>();
                        var info = new XmlSchemaInfo();

                        DataCenterNode ElementToNode(XElement element, DataCenterNode parent, bool top)
                        {
                            var name = element.Name;

                            validator.ValidateElement(
                                name.LocalName,
                                name.NamespaceName,
                                info,
                                null,
                                null,
                                top ? (string?)element.Attribute(xsi + "schemaLocation") : null,
                                null);

                            DataCenterNode current;

                            if (parent == root)
                                Monitor.Enter(root);

                            try
                            {
                                // This call has to be synchronized for the root element since multiple threads will be
                                // creating child nodes on it.
                                current = parent.CreateChild(element.Name.LocalName);
                            }
                            finally
                            {
                                if (parent == root)
                                    Monitor.Exit(root);
                            }

                            foreach (var attr in element
                                .Attributes()
                                .Where(a => !a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None))
                                current.AddAttribute(attr.Name.LocalName, AttributeToValue(attr));

                            validator.ValidateEndOfAttributes(null);

                            var keys = (info.SchemaElement?.UnhandledAttributes ?? Array.Empty<XmlAttribute>())
                                .Where(a => a.NamespaceURI == "https://vezel.dev/novadrop/dc" && a.LocalName == "keys")
                                .Select(a => a.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                                .Select(a =>
                                {
                                    Array.Resize(ref a, 4);

                                    return a;
                                })
                                .LastOrDefault();

                            if (keys != null && keys.Any(k => k != null))
                            {
                                var names = (keys[0], keys[1], keys[2], keys[3]);

                                ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                                    keyCache, names, out var exists);

                                if (!exists)
                                    entry = new(names.Item1, names.Item2, names.Item3, names.Item4);

                                current.Keys = entry!;
                            }

                            foreach (var node in element.Nodes())
                            {
                                switch (node)
                                {
                                    case XElement child:
                                        _ = ElementToNode(child, current, false);
                                        break;
                                    case XText text:
                                        validator.ValidateText(text.Value);
                                        break;
                                }
                            }

                            current.Value = validator.ValidateEndElement(null)?.ToString();

                            return current;
                        }

                        return (good, ElementToNode(doc.Root!, root, true));
                    }

                    await Parallel.ForEachAsync(
                        finalChanges,
                        cancellationToken,
                        async (tup, cancellationToken) =>
                        {
                            var path = tup.Key;

                            // We assume that if loading a sheet results in errors, it must be modified again to become
                            // successfully loadable. So we just discard the current change information.

                            switch (tup.Value)
                            {
                                case ChangeState.Created:
                                {
                                    var (good, node) = await LoadSheetAsync(new FileInfo(path));

                                    lock (root)
                                    {
                                        if (good)
                                            sheets.Add(path, node);
                                        else
                                            _ = root.RemoveChild(node);
                                    }

                                    break;
                                }

                                case ChangeState.Modified:
                                {
                                    var (good, node) = await LoadSheetAsync(new FileInfo(path));

                                    lock (root)
                                    {
                                        if (good)
                                        {
                                            var old = sheets[path];

                                            _ = root.RemoveChild(old);

                                            sheets[path] = node;
                                        }
                                        else
                                            _ = root.RemoveChild(node);
                                    }

                                    break;
                                }

                                case ChangeState.Deleted:
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

                    if (problems.Count != 0)
                    {
                        foreach (var nameGroup in problems.GroupBy(tup => tup.File.Directory!.Name))
                        {
                            Console.WriteLine($"{nameGroup.Key}/");

                            foreach (var fileGroup in nameGroup.GroupBy(tup => tup.File.Name))
                            {
                                var shownProblems = fileGroup.Take(10).ToArray();

                                Console.WriteLine($"  {fileGroup.Key}:");

                                foreach (var problem in shownProblems)
                                {
                                    var ex = problem.Args.Exception;
                                    var (msg, color) = problem.Args.Severity switch
                                    {
                                        XmlSeverityType.Error => ('E', ConsoleColor.Red),
                                        XmlSeverityType.Warning => ('W', ConsoleColor.Yellow),
                                        _ => throw new InvalidOperationException(), // Impossible.
                                    };

                                    Console.ForegroundColor = color;
                                    Console.WriteLine($"    [{msg}] ({ex.LineNumber},{ex.LinePosition}): {ex.Message}");
                                    Console.ResetColor();
                                }

                                var remainingProblems = fileGroup.Count() - shownProblems.Length;

                                if (remainingProblems != 0)
                                    Console.WriteLine($"    ... {remainingProblems} more problems ...");
                            }
                        }

                        Console.WriteLine();

                        continue;
                    }

                    sw1.Stop();

                    Console.WriteLine($"Data sheet changes applied in {sw1.Elapsed}.");
                    Console.WriteLine();
                    Console.WriteLine($"Packing to '{output}'...");

                    var sw2 = Stopwatch.StartNew();

                    await using var stream = File.Open(output.FullName, FileMode.Create, FileAccess.Write);

                    // TODO: What if this fails?
                    await dc.SaveAsync(
                        stream,
                        new DataCenterSaveOptions().WithCompressionLevel(level),
                        cancellationToken);

                    sw2.Stop();

                    Console.WriteLine($"Packed {dc.Root.Children.Count} data sheets in {sw2.Elapsed}.");
                    Console.WriteLine();
                }
            },
            inputArg,
            outputArg,
            levelOpt);
    }
}
