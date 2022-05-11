using Vezel.Novadrop.Data.IO;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;
using Vezel.Novadrop.Data.Serialization.Tables;

namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterWriter
{
    readonly DataCenterHeader _header = new();

    readonly DataCenterKeysTableWriter _keys;

    readonly DataCenterSegmentedRegion<DataCenterRawAttribute> _attributes = new();

    readonly DataCenterSegmentedRegion<DataCenterRawNode> _nodes = new();

    readonly DataCenterStringTableWriter _values = new(DataCenterConstants.ValueTableSize, false);

    readonly DataCenterStringTableWriter _names = new(DataCenterConstants.NameTableSize, true);

    readonly DataCenterFooter _footer = new();

    readonly Comparer<DataCenterNode> _comparer;

    readonly DataCenterSaveOptions _options;

    public DataCenterWriter(DataCenterSaveOptions options)
    {
        _keys = new(_names);
        _comparer = Comparer<DataCenterNode>.Create((a, b) =>
        {
            // Note that the node value attribute cannot be a key.
            var attrsA = new Dictionary<string, DataCenterValue>(a.Attributes);
            var attrsB = new Dictionary<string, DataCenterValue>(b.Attributes);

            int CompareBy(string? name)
            {
                return name != null ? attrsA.GetValueOrDefault(name).CompareTo(attrsB.GetValueOrDefault(name)) : 0;
            }

            var cmp = _names.GetString(a.Name).Index.CompareTo(_names.GetString(b.Name).Index);
            var keys = a.Keys;

            if (cmp == 0)
                cmp = CompareBy(keys.AttributeName1);

            if (cmp == 0)
                cmp = CompareBy(keys.AttributeName2);

            if (cmp == 0)
                cmp = CompareBy(keys.AttributeName3);

            if (cmp == 0)
                cmp = CompareBy(keys.AttributeName4);

            return cmp;
        });
        _options = options;
    }

    void ProcessTree(DataCenterNode root)
    {
        void AddNames(DataCenterNode node)
        {
            void AddName(string? name)
            {
                if (name is not (null or DataCenterConstants.RootNodeName or DataCenterConstants.ValueAttributeName))
                    _ = _names.AddString(name);
            }

            AddName(node.Name);

            var keys = node.Keys;

            // There can be keys that refer to attributes that do not exist even in the official data center, so we need
            // to explicitly add these attribute names.
            AddName(keys.AttributeName1);
            AddName(keys.AttributeName2);
            AddName(keys.AttributeName3);
            AddName(keys.AttributeName4);

            foreach (var (key, _) in node.Attributes)
                AddName(key);

            foreach (var child in node.Children)
                AddNames(child);
        }

        static DataCenterAddress InsertElements<T>(
            DataCenterSegmentedRegion<T> region,
            IReadOnlyCollection<T> items,
            string description)
            where T : unmanaged, IDataCenterItem<T>
        {
            var max = DataCenterAddress.MaxValue;

            if (items.Count == 0)
                return max;

            var segIdx = 0;
            var elemIdx = 0;
            var segment = default(DataCenterRegion<T>);

            // Try to find a region that the items can fit in.
            for (; segIdx < region.Segments.Count; segIdx++)
            {
                var seg = region.Segments[segIdx];

                if (seg.Elements.Count + items.Count <= max.ElementIndex)
                {
                    elemIdx = seg.Elements.Count;
                    segment = seg;

                    break;
                }
            }

            if (segment == null)
            {
                segIdx = region.Segments.Count;

                if (segIdx > max.SegmentIndex)
                    throw new InvalidOperationException($"{description} region is full ({segIdx} segments).");

                segment = new DataCenterRegion<T>();

                region.Segments.Add(segment);
            }

            foreach (var item in items)
                segment.Elements.Add(item);

            return new DataCenterAddress((ushort)segIdx, (ushort)elemIdx);
        }

        void EmitTree(DataCenterNode node, DataCenterAddress address)
        {
            var attributes = new Dictionary<string, DataCenterValue>(node.Attributes);

            if (node.Value is { IsNull: false } val)
                attributes.Add(DataCenterConstants.ValueAttributeName, val);

            var rawAttributes = attributes
                .Select(kvp => (_names.GetString(kvp.Key).Index, kvp.Value))
                .OrderBy(tup => tup.Index)
                .Select(tup =>
                {
                    var value = tup.Value;
                    var (code, ext) = value.TypeCode switch
                    {
                        DataCenterTypeCode.Int32 => (1, 0),
                        DataCenterTypeCode.Single => (2, 0),
                        DataCenterTypeCode.String => (3, DataCenterHash.ComputeValueHash(value.AsString)),
                        DataCenterTypeCode.Boolean => (1, 1),
                        _ => throw new InvalidOperationException(), // Impossible.
                    };

                    int result;

                    switch (value.TypeCode)
                    {
                        case DataCenterTypeCode.Int32:
                            result = value.AsInt32;
                            break;
                        case DataCenterTypeCode.Single:
                            var f = value.AsSingle;

                            result = Unsafe.As<float, int>(ref f);
                            break;
                        case DataCenterTypeCode.String:
                            var addr = _values.AddString(value.AsString).Address;
                            var segIdx = addr.SegmentIndex;
                            var elemIdx = addr.ElementIndex;

                            if (!BitConverter.IsLittleEndian)
                            {
                                segIdx = BinaryPrimitives.ReverseEndianness(segIdx);
                                elemIdx = BinaryPrimitives.ReverseEndianness(elemIdx);
                            }

                            result = elemIdx << 16 | segIdx;
                            break;
                        case DataCenterTypeCode.Boolean:
                            result = value.AsBoolean ? 1 : 0;
                            break;
                        default:
                            throw new InvalidOperationException(); // Impossible.
                    }

                    return new DataCenterRawAttribute
                    {
                        NameIndex = (ushort)tup.Index,
                        TypeInfo = (ushort)(ext << 2 | code),
                        Value = result,
                    };
                }).ToArray();

            var children = node.Children;
            var caddr = InsertElements(_nodes, new DataCenterRawNode[children.Count], "Node");

            foreach (var (i, child) in children.OrderBy(x => x, _comparer).Select((x, i) => (i, x)))
                EmitTree(child, new DataCenterAddress(caddr.SegmentIndex, (ushort)(caddr.ElementIndex + i)));

            var keys = node.Keys;
            var elem = new DataCenterRawNode
            {
                NameIndex = (ushort)_names.GetString(node.Name).Index,
                KeysInfo = (ushort)(_keys.AddKeys(
                    keys.AttributeName1,
                    keys.AttributeName2,
                    keys.AttributeName3,
                    keys.AttributeName4) << 4),
                AttributeCount = (ushort)attributes.Count,
                ChildCount = (ushort)children.Count,
                AttributeAddress = InsertElements(_attributes, rawAttributes, "Attribute"),
                ChildAddress = caddr,
            };

            _nodes.SetElement(address, elem);
        }

        // The tree needs to be sorted according to the index of name strings. So we must walk the entire tree and
        // ensure that all names have been added to the table before we actually write the tree.
        AddNames(root);

        // These must always go last and must always be present.
        _ = _names.AddString(DataCenterConstants.RootNodeName);
        _ = _names.AddString(DataCenterConstants.ValueAttributeName);

        EmitTree(root, InsertElements(_nodes, new DataCenterRawNode[1], "Node"));
    }

    [SuppressMessage("", "CA5401")]
    public Task WriteAsync(Stream stream, DataCenter center, CancellationToken cancellationToken)
    {
        return Task.Run(
            async () =>
            {
                ProcessTree(center.Root);

                // Write the uncompressed data center into memory first so that we can write the uncompressed size
                // before we write the zlib header. Sadness.
                using var memoryStream = new MemoryStream();

                var writer = new DataCenterBinaryWriter(memoryStream);

                await _header.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _keys.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _attributes.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _nodes.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _values.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _names.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
                await _footer.WriteAsync(writer, cancellationToken).ConfigureAwait(false);

                await memoryStream.FlushAsync(cancellationToken).ConfigureAwait(false);

                using var aes = DataCenter.CreateCipher(_options.Key, _options.IV);
                using var encryptor = aes.CreateEncryptor();
                var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write, true);

                await using (cryptoStream.ConfigureAwait(false))
                {
                    await new DataCenterBinaryWriter(cryptoStream)
                        .WriteUInt32Async((uint)memoryStream.Length, cancellationToken)
                        .ConfigureAwait(false);

                    var zlibStream = new ZLibStream(cryptoStream, _options.CompressionLevel, true);

                    await using (zlibStream.ConfigureAwait(false))
                    {
                        memoryStream.Position = 0;

                        await memoryStream.CopyToAsync(zlibStream, cancellationToken).ConfigureAwait(false);
                    }
                }
            },
            cancellationToken);
    }
}
