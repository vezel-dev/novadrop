// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Schema;

namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class SchemaCommand : CancellableAsyncCommand<SchemaCommand.SchemaCommandSettings>
{
    internal sealed class SchemaCommandSettings : CommandSettings
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
        public ReadOnlyMemory<byte> DecryptionKey { get; init; } = DataCenter.LatestKey;

        [CommandOption("--decryption-iv <iv>")]
        [Description("Set decryption IV")]
        [TypeConverter(typeof(HexStringConverter))]
        public ReadOnlyMemory<byte> DecryptionIV { get; init; } = DataCenter.LatestIV;

        [CommandOption("--architecture <architecture>")]
        [Description("Set format architecture")]
        public DataCenterArchitecture Architecture { get; init; } = DataCenterArchitecture.X64;

        [CommandOption("--strict")]
        [Description("Enable strict verification")]
        public bool Strict { get; init; }

        [CommandOption("--strategy <strategy>")]
        [Description("Set inference strategy")]
        public DataCenterSchemaStrategy Strategy { get; init; }

        [CommandOption("--subdirectories")]
        [Description("Enable output subdirectories based on data sheet names")]
        public bool Subdirectories { get; init; }

        public SchemaCommandSettings(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando, SchemaCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        Log.MarkupLine(
            """
            [yellow]Schemas generated by this command are not completely accurate.

            While schemas generated from a given data center file will correctly validate the
            exact data tree one might unpack from that file, certain nuances of the data can
            only be inferred on a best-effort basis. For example, it is impossible to accurately
            infer whether a given path with repeated node names in the tree is truly recursive,
            or whether an attribute that is always present is truly required.

            This means that schemas generated from this command may reject modifications to the
            data tree that are actually correct, e.g. as interpreted by the TERA client.

            Users who intend to modify a data center using schemas generated from this command
            should generate schemas with both the conservative and aggressive strategies, compare
            the resulting schemas, and construct a more accurate set of schemas using good human
            judgement.[/]
            """.ReplaceLineEndings());
        Log.WriteLine();
        Log.MarkupLineInterpolated(
            $"Inferring data sheet schemas of [cyan]{settings.Input}[/] to [cyan]{settings.Output}[/] with strategy [cyan]{settings.Strategy}[/]...");

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
                        .WithArchitecture(settings.Architecture)
                        .WithStrict(settings.Strict)
                        .WithLoaderMode(DataCenterLoaderMode.Eager)
                        .WithMutability(DataCenterMutability.Immutable),
                    cancellationToken);
            });

        var schema = await progress.RunTaskAsync(
            "Infer data sheet schemas",
            root.Children.Count,
            increment =>
                Task.FromResult(
                    DataCenterSchemaInference.Infer(settings.Strategy, root, increment, cancellationToken)));

        var output = new DirectoryInfo(settings.Output);

        await progress.RunTaskAsync(
            "Write data sheet schemas",
            schema.Children.Count + 1,
            async increment =>
            {
                output.Create();

                var sheetOutput = settings.Subdirectories ? output : output.CreateSubdirectory("sheets");
                var xmlSettings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    IndentChars = "    ",
                    NewLineHandling = NewLineHandling.Entitize,
                    Async = true,
                };

                await Parallel.ForEachAsync(
                    schema.Children,
                    cancellationToken,
                    async (item, cancellationToken) =>
                    {
                        await WriteSchemaAsync(
                            settings.Subdirectories ? sheetOutput.CreateSubdirectory(item.Key) : sheetOutput,
                            item.Key,
                            xmlSettings,
                            item.Value.Node,
                            cancellationToken);

                        increment();
                    });

#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
                await using var inXsd = typeof(ThisAssembly).Assembly.GetManifestResourceStream("DataCenter.xsd");
#pragma warning restore CS0436

                if (inXsd != null)
                {
                    await using var outXsd = File.Open(
                        Path.Combine(output.FullName, "DataCenter.xsd"), FileMode.Create, FileAccess.Write);

                    await inXsd.CopyToAsync(outXsd, cancellationToken);
                }

                increment();
            });

        return 0;
    }

    private static async Task WriteSchemaAsync(
        DirectoryInfo directory,
        string name,
        XmlWriterSettings xmlSettings,
        DataCenterNodeSchema nodeSchema,
        CancellationToken cancellationToken)
    {
        var writtenTypes = new Dictionary<DataCenterNodeSchema, string>();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteAttributesAsync(XmlWriter xmlWriter, DataCenterNodeSchema nodeSchema)
        {
            if (nodeSchema.Attributes.Count == 0)
                return;

            foreach (var (name, attrSchema) in nodeSchema.Attributes.OrderBy(static n => n.Key, StringComparer.Ordinal))
            {
                await xmlWriter.WriteStartElementAsync("xsd", "attribute", ns: null);

                {
                    await xmlWriter.WriteAttributeStringAsync(prefix: null, "name", ns: null, name);
                    await xmlWriter.WriteAttributeStringAsync(prefix: null, "type", ns: null, attrSchema.TypeCode switch
                    {
                        DataCenterTypeCode.Int32 => "xsd:int",
                        DataCenterTypeCode.Single => "xsd:float",
                        DataCenterTypeCode.String => "xsd:string",
                        DataCenterTypeCode.Boolean => "xsd:boolean",
                        _ => throw new UnreachableException(),
                    });

                    if (!attrSchema.IsOptional)
                        await xmlWriter.WriteAttributeStringAsync(prefix: null, "use", ns: null, "required");
                }

                await xmlWriter.WriteEndElementAsync();
            }
        }

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteComplexTypeAsync(XmlWriter xmlWriter, string typeName, DataCenterNodeSchema nodeSchema)
        {
            cancellationToken.ThrowIfCancellationRequested();

            writtenTypes.Add(nodeSchema, typeName);

            await xmlWriter.WriteStartElementAsync("xsd", "complexType", ns: null);

            {
                await xmlWriter.WriteAttributeStringAsync(prefix: null, "name", ns: null, typeName);

                if (nodeSchema.HasKeys)
                    await xmlWriter.WriteAttributeStringAsync(
                        "dc", "keys", null, string.Join(" ", nodeSchema.Keys.Distinct()));

                if (nodeSchema.HasMixedContent)
                    await xmlWriter.WriteAttributeStringAsync(prefix: null, "mixed", ns: null, "true");

                if (nodeSchema.Children.Count != 0)
                {
                    await xmlWriter.WriteStartElementAsync("xsd", "sequence", ns: null);

                    foreach (var (childName, edge) in nodeSchema
                        .Children
                        .OrderBy(static n => n.Key, StringComparer.Ordinal))
                    {
                        var fullChildName = writtenTypes.GetValueOrDefault(edge.Node) ?? $"{typeName}_{childName}";

                        await xmlWriter.WriteStartElementAsync("xsd", "element", ns: null);
                        await xmlWriter.WriteAttributeStringAsync(prefix: null, "name", ns: null, childName);
                        await xmlWriter.WriteAttributeStringAsync(prefix: null, "type", ns: null, fullChildName);

                        if (edge.IsOptional)
                            await xmlWriter.WriteAttributeStringAsync(prefix: null, "minOccurs", ns: null, "0");

                        if (edge.IsRepeatable)
                            await xmlWriter.WriteAttributeStringAsync(prefix: null, "maxOccurs", ns: null, "unbounded");

                        await xmlWriter.WriteEndElementAsync();
                    }

                    await xmlWriter.WriteEndElementAsync();
                }

                if (nodeSchema.HasValue && !nodeSchema.HasMixedContent)
                {
                    await xmlWriter.WriteStartElementAsync("xsd", "simpleContent", ns: null);

                    {
                        await xmlWriter.WriteStartElementAsync("xsd", "extension", ns: null);

                        {
                            await xmlWriter.WriteAttributeStringAsync(prefix: null, "base", ns: null, "xsd:string");

                            await WriteAttributesAsync(xmlWriter, nodeSchema);
                        }

                        await xmlWriter.WriteEndElementAsync();
                    }

                    await xmlWriter.WriteEndElementAsync();
                }
                else
                    await WriteAttributesAsync(xmlWriter, nodeSchema);
            }

            await xmlWriter.WriteEndElementAsync();

            if (nodeSchema.Children.Count != 0)
                foreach (var (childName, child) in nodeSchema
                    .Children
                    .OrderBy(static n => n.Key, StringComparer.Ordinal))
                    if (!writtenTypes.ContainsKey(child.Node))
                        await WriteComplexTypeAsync(xmlWriter, $"{typeName}_{childName}", child.Node);
        }

        await using var textWriter = new StreamWriter(Path.Combine(directory.FullName, $"{name}.xsd"));

        await using (var xmlWriter = XmlWriter.Create(textWriter, xmlSettings))
        {
            const string baseUri = "https://vezel.dev/novadrop/dc";

            await xmlWriter.WriteStartElementAsync("xsd", "schema", "http://www.w3.org/2001/XMLSchema");

            {
                await xmlWriter.WriteAttributeStringAsync(
                    "xmlns", "xsd", ns: null, "http://www.w3.org/2001/XMLSchema");
                await xmlWriter.WriteAttributeStringAsync(
                    "xmlns", "xsi", ns: null, "http://www.w3.org/2001/XMLSchema-instance");
                await xmlWriter.WriteAttributeStringAsync("xmlns", "dc", ns: null, baseUri);
                await xmlWriter.WriteAttributeStringAsync(prefix: null, "xmlns", ns: null, $"{baseUri}/{name}");
                await xmlWriter.WriteAttributeStringAsync(
                    prefix: null, "targetNamespace", ns: null, $"{baseUri}/{name}");
                await xmlWriter.WriteAttributeStringAsync(
                    "xsi", "schemaLocation", ns: null, $"{baseUri} ../DataCenter.xsd");
                await xmlWriter.WriteAttributeStringAsync(prefix: null, "elementFormDefault", ns: null, "qualified");

                await WriteComplexTypeAsync(xmlWriter, name, nodeSchema);

                await xmlWriter.WriteStartElementAsync("xsd", "element", ns: null);

                {
                    await xmlWriter.WriteAttributeStringAsync(prefix: null, "name", ns: null, name);
                    await xmlWriter.WriteAttributeStringAsync(prefix: null, "type", ns: null, name);
                }

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
        }

        await textWriter.WriteLineAsync();
    }
}
