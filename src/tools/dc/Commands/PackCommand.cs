namespace Vezel.Novadrop.Commands;

sealed class PackCommand : Command
{
    public PackCommand()
        : base("pack", "Pack the contents of a directory to a data center file.")
    {
        var inputArg = new Argument<DirectoryInfo>("input", "Input directory");
        var outputArg = new Argument<FileInfo>("output", "Output file");
        var levelOpt = new Option<CompressionLevel>(
            "--compression",
            () => CompressionLevel.Optimal,
            "Set compression level");

        Add(inputArg);
        Add(outputArg);
        Add(levelOpt);

        this.SetHandler(
            async (
                InvocationContext context,
                DirectoryInfo input,
                FileInfo output,
                CompressionLevel level,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Packing '{input}' to '{output}'...");

                var sw = Stopwatch.StartNew();

                var dc = DataCenter.Create();
                var root = dc.Root;

                var files = input.EnumerateFiles("?*-?*.xml", SearchOption.AllDirectories).ToArray();
                var problems = new List<(string Name, FileInfo File, ValidationEventArgs Args)>();

                var xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";

                await Parallel.ForEachAsync(
                    files,
                    cancellationToken,
                    async (file, cancellationToken) =>
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

                        validator.ValidationEventHandler += (_, e) =>
                        {
                            lock (problems)
                                problems.Add((file.Directory!.Name, file, e));
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

                        void ElementToNode(XElement element, DataCenterNode parent, bool top)
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
                                        ElementToNode(child, current, false);
                                        break;
                                    case XText text:
                                        validator.ValidateText(text.Value);
                                        break;
                                }
                            }

                            // TODO: This is wrong; we should only support string and mixed content.
                            current.Value = validator.ValidateEndElement(null) switch
                            {
                                int i => i,
                                float f => f,
                                string s => s,
                                bool b => b,
                                _ => default,
                            };
                        }

                        ElementToNode(doc.Root!, root, true);
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

                    context.ExitCode = 1;

                    return;
                }

                await using var stream = File.Open(output.FullName, FileMode.Create, FileAccess.Write);

                await dc.SaveAsync(
                    stream,
                    new DataCenterSaveOptions().WithCompressionLevel(level),
                    cancellationToken);

                sw.Stop();

                Console.WriteLine($"Packed {files.Length} data sheets in {sw.Elapsed}.");
            },
            inputArg,
            outputArg,
            levelOpt);
    }
}
