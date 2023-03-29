namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class PackCommand : CancellableAsyncCommand<PackCommand.PackCommandSettings>
{
    public sealed class PackCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input directory")]
        public string Input { get; }

        [CommandArgument(1, "<output>")]
        [Description("Output file")]
        public string Output { get; }

        [CommandOption("--revision <value>")]
        [Description("Set data revision")]
        public int Revision { get; init; } = DataCenter.LatestRevision;

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

        public PackCommandSettings(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    protected override Task PreExecuteAsync(
        dynamic expando, PackCommandSettings settings, CancellationToken cancellationToken)
    {
        expando.Handler = new DataSheetValidationHandler();

        return Task.CompletedTask;
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando, PackCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.MarkupLineInterpolated($"Packing [cyan]{settings.Input}[/] to [cyan]{settings.Output}[/]...");

        var files = await progress.RunTaskAsync(
            "Gather data sheet files",
            () => Task.FromResult(
                new DirectoryInfo(settings.Input)
                    .EnumerateFiles("?*-?*.xml", SearchOption.AllDirectories)
                    .OrderBy(f => f.FullName, StringComparer.Ordinal)
                    .Select((file, index) => (index, file))
                    .ToArray()));

        var root = DataCenter.Create();
        var xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";
        var handler = (DataSheetValidationHandler)expando.Handler;

        var nodes = await progress.RunTaskAsync(
            "Load data sheets",
            files.Length,
            increment => Task.WhenAll(
                files
                    .AsParallel()
                    .WithCancellation(cancellationToken)
                    .Select(item => Task.Run(
                        async () =>
                        {
                            var file = item.file;
                            var xmlSettings = new XmlReaderSettings
                            {
                                Async = true,
                            };

                            using var reader = XmlReader.Create(file.FullName, xmlSettings);

                            XDocument doc;

                            try
                            {
                                doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
                            }
                            catch (XmlException ex)
                            {
                                handler.HandleException(file, ex);

                                return (item.index, node: default(DataCenterNode));
                            }

                            // We need to access type and key info from the schema during tree construction, so we do
                            // the validation manually as we go rather than relying on validation support in XmlReader
                            // or XDocument. (Notably, the latter also has a very broken implementation that does not
                            // respect xsi:schemaLocation, even with an XmlUrlResolver set...)
                            var validator = new XmlSchemaValidator(
                                reader.NameTable,
                                xmlSettings.Schemas,
                                new XmlNamespaceManager(reader.NameTable),
                                XmlSchemaValidationFlags.ProcessSchemaLocation |
                                XmlSchemaValidationFlags.ReportValidationWarnings)
                            {
                                XmlResolver = new XmlUrlResolver(),
                                SourceUri = new Uri(reader.BaseURI),
                            };

                            validator.ValidationEventHandler += handler.GetEventHandlerFor(file);

                            validator.Initialize();

                            var keyCache = new Dictionary<(string?, string?, string?, string?), DataCenterKeys>();
                            var info = new XmlSchemaInfo();

                            DataCenterNode ElementToNode(XElement element, DataCenterNode parent, bool top)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

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

                                var locked = false;

                                // Multiple threads will be creating children on the root node so we need to lock.
                                if (parent == root)
                                    Monitor.Enter(root, ref locked);

                                try
                                {
                                    current = parent.CreateChild(name.LocalName);
                                }
                                finally
                                {
                                    if (locked)
                                        Monitor.Exit(root);
                                }

                                foreach (var attr in element.Attributes().Where(
                                    a => !a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None))
                                {
                                    var attrName = attr.Name;
                                    var attrValue = validator.ValidateAttribute(
                                        attrName.LocalName, attrName.NamespaceName, attr.Value, null)!;

                                    current.AddAttribute(attr.Name.LocalName, attrValue switch
                                    {
                                        int i => i,
                                        float f => f,
                                        string s => s,
                                        bool b => b,
                                        _ => 42, // Dummy value in case of validation failure.
                                    });
                                }

                                validator.ValidateEndOfAttributes(null);

                                if (info.SchemaElement?.ElementSchemaType?.UnhandledAttributes is { Length: not 0 } un)
                                {
                                    var names = un
                                        .Where(a =>
                                            a.NamespaceURI == "https://vezel.dev/novadrop/dc" && a.LocalName == "keys")
                                        .Select(a => a.Value.Split(
                                            ' ',
                                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                        .Select(arr =>
                                        {
                                            Array.Resize(ref arr, 4);

                                            return (arr[0], arr[1], arr[2], arr[3]);
                                        })
                                        .LastOrDefault();

                                    if (names is not (null, null, null, null))
                                    {
                                        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                                            keyCache, names, out var exists);

                                        if (!exists)
                                            entry = new(names.Item1, names.Item2, names.Item3, names.Item4);

                                        current.Keys = entry!;
                                    }
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

                                var value = validator.ValidateEndElement(null)?.ToString();

                                if (!string.IsNullOrEmpty(value))
                                    current.Value = value;

                                return current;
                            }

                            var node = ElementToNode(doc.Root!, root, true);

                            increment();

                            return (item.index, node);
                        },
                        cancellationToken))));

        if (handler.HasProblems)
            return 1;

        await progress.RunTaskAsync(
            "Sort root child nodes",
            () =>
            {
                var lookup = nodes.ToDictionary(item => item.node!, item => item.index);

                // Since we process data sheets in parallel (i.e. non-deterministically), the data center we now have in
                // memory will not have the correct order for the immediate children of the root node. We fix that here.
                root.SortChildren(Comparer<DataCenterNode>.Create((x, y) => lookup[x].CompareTo(lookup[y])));

                return Task.CompletedTask;
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
                        .WithRevision(settings.Revision)
                        .WithCompressionLevel(settings.Compression)
                        .WithKey(settings.EncryptionKey.Span)
                        .WithIV(settings.EncryptionIV.Span),
                    cancellationToken);
            });

        return 0;
    }

    protected override Task PostExecuteAsync(
        dynamic expando, PackCommandSettings settings, CancellationToken cancellationToken)
    {
        expando.Handler.Print();

        return Task.CompletedTask;
    }
}
