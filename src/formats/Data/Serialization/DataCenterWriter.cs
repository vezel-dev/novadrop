using Vezel.Novadrop.Cryptography;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;
using Vezel.Novadrop.Data.Serialization.Tables;

namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterWriter
{
    readonly DataCenterHeader _header;

    readonly DataCenterKeysTableWriter _keys;

    readonly DataCenterSegmentedRegion<DataCenterRawAttribute> _attributes = new();

    readonly DataCenterSegmentedRegion<DataCenterRawNode> _nodes = new();

    readonly DataCenterStringTableWriter _values = new(DataCenterConstants.ValueTableSize, false);

    readonly DataCenterStringTableWriter _names = new(DataCenterConstants.NameTableSize, true);

    readonly DataCenterFooter _footer = new();

    readonly DataCenterSaveOptions _options;

    public DataCenterWriter(DataCenterSaveOptions options)
    {
        _header = new()
        {
            Revision = options.Revision,
        };
        _keys = new(_names);
        _options = options;
    }

    void ProcessTree(DataCenterNode root, CancellationToken cancellationToken)
    {
        static DataCenterAddress AllocateRange<T>(DataCenterSegmentedRegion<T> region, int count, string description)
            where T : unmanaged, IDataCenterItem
        {
            var max = DataCenterAddress.MaxValue;
            var segIdx = 0;
            var elemIdx = 0;
            var segment = default(DataCenterRegion<T>);

            // Try to find a region that the items can fit in.
            for (; segIdx < region.Segments.Count; segIdx++)
            {
                var seg = region.Segments[segIdx];

                if (seg.Elements.Count + count <= max.ElementIndex)
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

            for (var i = 0; i < count; i++)
                segment.Elements.Add(default);

            return new((ushort)segIdx, (ushort)elemIdx);
        }

        var comparer = Comparer<DataCenterNode>.Create((x, y) =>
        {
            var cmp = _names.GetString(x.Name).Index.CompareTo(_names.GetString(y.Name).Index);

            if (!(x.HasAttributes || y.HasAttributes))
                return cmp;

            // Note that the node value attribute cannot be a key.
            var attrsA = x.Attributes;
            var attrsB = y.Attributes;

            int CompareBy(string? name)
            {
                return name != null ? attrsA.GetValueOrDefault(name).CompareTo(attrsB.GetValueOrDefault(name)) : 0;
            }

            var keys = x.Keys;

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

        void WriteTree(DataCenterNode node, DataCenterAddress address)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attrCount = 0;
            var attrAddr = DataCenterAddress.MaxValue;

            if (node.HasAttributes || node.Value != null)
            {
                var attributes = new List<(int Index, DataCenterValue Value)>();

                void AddAttribute(string name, DataCenterValue value)
                {
                    attributes.Add(new(_names.GetString(name).Index, value));
                }

                if (node.HasAttributes)
                    foreach (var (name, value) in node.Attributes)
                        AddAttribute(name, value);

                if (node.Value != null)
                    AddAttribute(DataCenterConstants.ValueAttributeName, node.Value);

                attributes.Sort((x, y) => x.Index.CompareTo(y.Index));

                attrCount = attributes.Count;
                attrAddr = AllocateRange(_attributes, attrCount, "Attribute");

                var i = 0;

                foreach (var (index, value) in attributes)
                {
                    var (code, ext) = value.TypeCode switch
                    {
                        DataCenterTypeCode.Int32 => (1, 0),
                        DataCenterTypeCode.Single => (2, 0),
                        DataCenterTypeCode.String => (3, DataCenterHash.ComputeValueHash(value.UnsafeAsString)),
                        DataCenterTypeCode.Boolean => (1, 1),
                        _ => throw new UnreachableException(),
                    };

                    int result;

                    switch (value.TypeCode)
                    {
                        // DataCenterValue internally normalizes values of these types to the representation we want to
                        // write to the attribute, so we can just (re)interpret the value as int.
                        case DataCenterTypeCode.Int32:
                        case DataCenterTypeCode.Single:
                        case DataCenterTypeCode.Boolean:
                            result = value.UnsafeAsInt32;
                            break;
                        case DataCenterTypeCode.String:
                            var addr = _values.AddString(value.UnsafeAsString).Address;
                            var segIdx = addr.SegmentIndex;
                            var elemIdx = addr.ElementIndex;

                            if (!BitConverter.IsLittleEndian)
                            {
                                segIdx = BinaryPrimitives.ReverseEndianness(segIdx);
                                elemIdx = BinaryPrimitives.ReverseEndianness(elemIdx);
                            }

                            result = elemIdx << 16 | segIdx;
                            break;
                        default:
                            throw new UnreachableException();
                    }

                    _attributes.SetElement(new(attrAddr.SegmentIndex, (ushort)(attrAddr.ElementIndex + i)), new()
                    {
                        NameIndex = (ushort)index,
                        TypeInfo = (ushort)(ext << 2 | code),
                        Value = result,
                    });

                    i++;
                }
            }

            var childCount = 0;
            var childAddr = DataCenterAddress.MaxValue;

            if (node.HasChildren)
            {
                var children = node.Children;

                childCount = children.Count;
                childAddr = AllocateRange(_nodes, childCount, "Node");

                foreach (var (i, child) in children.OrderBy(n => n, comparer).Select((n, i) => (i, n)))
                    WriteTree(child, new(childAddr.SegmentIndex, (ushort)(childAddr.ElementIndex + i)));
            }

            var keys = node.Keys;

            _nodes.SetElement(address, new()
            {
                NameIndex = (ushort)_names.GetString(node.Name).Index,
                KeysInfo = (ushort)(_keys.AddKeys(
                    keys.AttributeName1,
                    keys.AttributeName2,
                    keys.AttributeName3,
                    keys.AttributeName4) << 4),
                AttributeCount = (ushort)attrCount,
                ChildCount = (ushort)childCount,
                AttributeAddress = attrAddr,
                ChildAddress = childAddr,
            });
        }

        // The tree needs to be sorted according to the index of name strings. So we must walk the entire tree and
        // ensure that all names have been added to the table before we actually write the tree.
        DataCenterNameTree.Collect(root, s => _names.AddString(s), cancellationToken);

        WriteTree(root, AllocateRange(_nodes, 1, "Node"));
    }

    [SuppressMessage("", "CA5401")]
    public Task WriteAsync(Stream stream, DataCenterNode root, CancellationToken cancellationToken)
    {
        return Task.Run(
            async () =>
            {
                ProcessTree(root, cancellationToken);

                // Write the uncompressed data center into memory first so that we can write the uncompressed size
                // before we write the zlib header. Sadness.
                using var memoryStream = new MemoryStream();

                var writer = new StreamBinaryWriter(memoryStream);

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
                using var padder = new FakePaddingCryptoTransform(encryptor);
                var cryptoStream = new CryptoStream(stream, padder, CryptoStreamMode.Write, true);

                await using (cryptoStream.ConfigureAwait(false))
                {
                    await new StreamBinaryWriter(cryptoStream)
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
