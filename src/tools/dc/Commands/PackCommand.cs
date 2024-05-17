// SPDX-License-Identifier: 0BSD

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

        [CommandOption("--format <format>")]
        [Description("Set format variant")]
        public DataCenterFormat Format { get; init; } = DataCenterFormat.V6X64;

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

    private static readonly XName _schemaLocation =
        XName.Get("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");

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
                    .OrderBy(static f => f.FullName, StringComparer.Ordinal)
                    .Select(static (file, index) => (File: file, Index: index))
                    .ToArray()));

        var root = DataCenter.Create();
        var handler = (DataSheetValidationHandler)expando.Handler;

        var nodes = await progress.RunTaskAsync(
            "Load data sheets",
            files.Length,
            async increment =>
            {
                var keyCache = new ConcurrentDictionary<(string?, string?, string?, string?), DataCenterKeys>();
                var nodes = new ConcurrentBag<(DataCenterNode Node, int Index)>();

                await Parallel.ForEachAsync(
                    files,
                    cancellationToken,
                    async (tup, cancellationToken) =>
                    {
                        var file = tup.File;
                        var xmlSettings = new XmlReaderSettings
                        {
                            Async = true,
                        };

                        using var reader = XmlReader.Create(file.FullName, xmlSettings);

                        XDocument doc;

                        try
                        {
                            doc = await XDocument.LoadAsync(reader, LoadOptions.SetLineInfo, cancellationToken);
                        }
                        catch (XmlException ex)
                        {
                            handler.HandleException(file, ex);

                            return;
                        }

                        // We need to access type and key info from the schema during tree construction, so we do the
                        // validation manually as we go rather than relying on validation support in XmlReader or
                        // XDocument. (Notably, the latter also has a very broken implementation that does not respect
                        // xsi:schemaLocation, even with an XmlUrlResolver set...)
                        var validator = new XmlSchemaValidator(
                            reader.NameTable,
                            xmlSettings.Schemas,
                            new XmlNamespaceManager(reader.NameTable),
                            XmlSchemaValidationFlags.ProcessSchemaLocation |
                            XmlSchemaValidationFlags.ReportValidationWarnings)
                        {
                            XmlResolver = new XmlUrlResolver(),
                            SourceUri = new Uri(reader.BaseURI),
                            LineInfoProvider = doc,
                        };

                        validator.ValidationEventHandler += handler.GetEventHandlerFor(file);

                        validator.Initialize();

                        var info = new XmlSchemaInfo();

                        DataCenterNode ElementToNode(XElement element, DataCenterNode parent, bool top)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var name = element.Name;

                            validator.ValidateElement(
                                name.LocalName,
                                name.NamespaceName,
                                info,
                                xsiType: null,
                                xsiNil: null,
                                top ? (string?)element.Attribute(_schemaLocation) : null,
                                xsiNoNamespaceSchemaLocation: null);

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

                            foreach (var attr in element
                                .Attributes()
                                .Where(static a => !a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None))
                            {
                                var attrName = attr.Name;
                                var attrValue = validator.ValidateAttribute(
                                    attrName.LocalName, attrName.NamespaceName, attr.Value, schemaInfo: null)!;

                                current.AddAttribute(attr.Name.LocalName, attrValue switch
                                {
                                    int i => i,
                                    float f => f,
                                    string s => s,
                                    bool b => b,
                                    _ => 42, // Dummy value in case of validation failure.
                                });
                            }

                            validator.ValidateEndOfAttributes(schemaInfo: null);

                            if (info.SchemaElement?.ElementSchemaType?.UnhandledAttributes is [_, ..] unhandled)
                            {
                                var names = unhandled
                                    .Where(static a =>
                                        a.NamespaceURI == "https://vezel.dev/novadrop/dc" && a.LocalName == "keys")
                                    .Select(static a => a.Value.Split(
                                        ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                    .Select(static arr =>
                                    {
                                        Array.Resize(ref arr, 4);

                                        return (arr[0], arr[1], arr[2], arr[3]);
                                    })
                                    .LastOrDefault();

                                if (names is not (null, null, null, null))
                                    current.Keys = keyCache.GetOrAdd(
                                        names, static names => new(names.Item1, names.Item2, names.Item3, names.Item4));
                            }

                            foreach (var node in element.Nodes())
                            {
                                switch (node)
                                {
                                    case XElement child:
                                        _ = ElementToNode(child, current, top: false);
                                        break;
                                    case XText text:
                                        validator.ValidateText(text.Value);
                                        break;
                                }
                            }

                            var value = validator.ValidateEndElement(schemaInfo: null)?.ToString();

                            if (!string.IsNullOrEmpty(value))
                                current.Value = value;

                            return current;
                        }

                        var node = ElementToNode(doc.Root!, root, top: true);

                        increment();

                        nodes.Add((node, tup.Index));
                    });

                return nodes;
            });

        if (handler.HasDiagnostics)
            return 1;

        await progress.RunTaskAsync(
            "Sort root child nodes",
            () =>
            {
                var lookup = nodes.ToDictionary(static item => item.Node, static item => item.Index);

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
                        .WithFormat(settings.Format)
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
