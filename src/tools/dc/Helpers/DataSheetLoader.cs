namespace Vezel.Novadrop.Helpers;

static class DataSheetLoader
{
    static readonly XNamespace _xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";

    public static async Task<DataCenterNode?> LoadAsync(
        FileInfo file,
        DataSheetValidationHandler handler,
        DataCenterNode root,
        CancellationToken cancellationToken)
    {
        var settings = new XmlReaderSettings
        {
            Async = true,
        };

        using var reader = XmlReader.Create(file.FullName, settings);

        var doc = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);

        // We need to access type and key info from the schema during tree construction, so we do the validation
        // manually as we go rather than relying on validation support in XmlReader or XDocument. (Notably, the latter
        // also has a very broken implementation that does not respect xsi:schemaLocation, even with an XmlUrlResolver
        // set...)
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

        var eventHandler = handler.GetEventHandlerFor(file);
        var good = true;

        validator.ValidationEventHandler += (sender, e) =>
        {
            eventHandler(sender, e);

            good = false;
        };

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
                top ? (string?)element.Attribute(_xsi + "schemaLocation") : null,
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

            if (info.SchemaElement?.UnhandledAttributes is XmlAttribute[] unAttrs)
            {
                var keys = unAttrs
                    .Where(a => a.NamespaceURI == "https://vezel.dev/novadrop/dc" && a.LocalName == "keys")
                    .Select(a => a.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Select(arr =>
                    {
                        Array.Resize(ref arr, 4);

                        return arr;
                    })
                    .LastOrDefault();

                if (keys != null && keys.Any(k => k != null))
                {
                    var names = (keys[0], keys[1], keys[2], keys[3]);

                    if (!keyCache.TryGetValue(names, out var entry))
                        keyCache.Add(names, entry = new(names.Item1, names.Item2, names.Item3, names.Item4));

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

        if (!good)
        {
            Monitor.Enter(root);

            try
            {
                _ = root.RemoveChild(node);
            }
            finally
            {
                Monitor.Exit(root);
            }

            return null;
        }

        return node;
    }
}
