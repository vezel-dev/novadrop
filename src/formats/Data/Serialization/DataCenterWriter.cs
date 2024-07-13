using Vezel.Novadrop.Cryptography;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;
using Vezel.Novadrop.Data.Serialization.Tables;

namespace Vezel.Novadrop.Data.Serialization;

internal sealed class DataCenterWriter
{
    private const int StackBufferSize = 128;

    private static readonly OrderedDictionary<string, DataCenterValue> _emptyAttributes = [];

    private readonly DataCenterHeader _header;

    private readonly DataCenterKeysTableWriter _keys;

    private readonly DataCenterSegmentedRegion<DataCenterRawAttribute> _attributes = new();

    private readonly Dictionary<int, List<(DataCenterAddress, int)>> _attributeCache = [];

    private readonly DataCenterSegmentedRegion<DataCenterRawNode> _nodes = new();

    private readonly Dictionary<int, List<(DataCenterAddress, int)>> _nodeCache = [];

    private readonly DataCenterStringTableWriter _values = new(DataCenterConstants.ValueTableSize, false);

    private readonly DataCenterStringTableWriter _names = new(DataCenterConstants.NameTableSize, true);

    private readonly DataCenterFooter _footer = new();

    private readonly DataCenterSaveOptions _options;

    public DataCenterWriter(DataCenterSaveOptions options)
    {
        _header = new()
        {
            Revision = options.Revision,
        };
        _keys = new(_names);
        _options = options;
    }

    private void ProcessTree(DataCenterNode root, CancellationToken cancellationToken)
    {
        static int GetItemsHashCode<T>(ReadOnlySpan<T> items)
        {
            var hash = default(HashCode);

            foreach (var item in items)
                hash.Add(item);

            return hash.ToHashCode();
        }

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

                Check.Operation(segIdx <= max.SegmentIndex, $"{description} region is full ({segIdx} segments).");

                segment = new();

                region.Segments.Add(segment);
            }

            for (var i = 0; i < count; i++)
                segment.Elements.Add(default);

            return new((ushort)segIdx, (ushort)elemIdx);
        }

        static void WriteRange<T>(
            DataCenterSegmentedRegion<T> region,
            Dictionary<int, List<(DataCenterAddress Address, int Count)>> cache,
            ReadOnlySpan<T> items,
            DataCenterAddress address)
            where T : unmanaged, IDataCenterItem, IEquatable<T>
        {
            var i = 0;

            foreach (var item in items)
            {
                region.SetElement(new(address.SegmentIndex, (ushort)(address.ElementIndex + i)), item);

                i++;
            }

            ref var ranges = ref CollectionsMarshal.GetValueRefOrAddDefault(cache, GetItemsHashCode(items), out _);

            (ranges ??= []).Add((address, items.Length));
        }

        static bool TryDeduplicateItems<T>(
            DataCenterSegmentedRegion<T> region,
            Dictionary<int, List<(DataCenterAddress Address, int Count)>> cache,
            ReadOnlySpan<T> items,
            out DataCenterAddress address)
            where T : unmanaged, IDataCenterItem, IEquatable<T>
        {
            if (cache.TryGetValue(GetItemsHashCode(items), out var ranges))
            {
                foreach (var (cachedAddr, cachedCount) in ranges)
                {
                    if (items.Length != cachedCount)
                        continue;

                    var i = 0;
                    var good = true;

                    foreach (var element in items)
                    {
                        var cachedElement = region.GetElement(
                            new(cachedAddr.SegmentIndex, (ushort)(cachedAddr.ElementIndex + i)));

                        if (!element.Equals(cachedElement))
                        {
                            good = false;

                            break;
                        }

                        i++;
                    }

                    if (!good)
                        continue;

                    address = cachedAddr;

                    return true;
                }
            }

            address = DataCenterAddress.MaxValue;

            return false;
        }

        var comparison = new Comparison<(int Index, DataCenterNode Node)>(
            Comparer<(int Position, DataCenterNode Node)>.Create((x, y) =>
            {
                var (posX, nodeX) = x;
                var (posY, nodeY) = y;

                var cmp = _names.GetString(nodeX.Name).Index.CompareTo(_names.GetString(nodeY.Name).Index);

                if (nodeX.HasAttributes || nodeY.HasAttributes)
                {
                    // Note that the node value attribute cannot be a key.

                    var attrsA = nodeX.Attributes;
                    var attrsB = nodeY.Attributes;

                    int CompareBy(string? name)
                    {
                        return name != null
                            ? attrsA.GetValueOrDefault(name).CompareTo(attrsB.GetValueOrDefault(name))
                            : 0;
                    }

                    var keys = nodeX.Keys;

                    if (cmp == 0)
                        cmp = CompareBy(keys.AttributeName1);

                    if (cmp == 0)
                        cmp = CompareBy(keys.AttributeName2);

                    if (cmp == 0)
                        cmp = CompareBy(keys.AttributeName3);

                    if (cmp == 0)
                        cmp = CompareBy(keys.AttributeName4);
                }

                // Node sorting must be stable.
                if (cmp == 0)
                    cmp = posX.CompareTo(posY);

                return cmp;
            }).Compare);

        DataCenterRawNode WriteTree(DataCenterNode node)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int attrCount;
            DataCenterAddress attrAddr;

            if (node.HasAttributes || node.Value != null)
            {
                var nodeAttributes = node.HasAttributes ? node.UnsafeAttributes : _emptyAttributes;

                attrCount = nodeAttributes.Count;

                if (node.Value != null)
                    attrCount++;

                var sortedAttributes = new (int Index, DataCenterValue Value)[attrCount];

                void AddAttribute(int position, string name, DataCenterValue value)
                {
                    sortedAttributes[position] = (_names.GetString(name).Index, value);
                }

                if (node.HasAttributes)
                {
                    var i = 0;

                    foreach (var (name, value) in nodeAttributes)
                    {
                        AddAttribute(i, name, value);

                        i++;
                    }
                }

                if (node.Value != null)
                    AddAttribute(attrCount - 1, DataCenterConstants.ValueAttributeName, node.Value);

                Array.Sort(sortedAttributes, static (x, y) => x.Index.CompareTo(y.Index));

                var rawAttributes = attrCount <= StackBufferSize
                    ? stackalloc DataCenterRawAttribute[attrCount]
                    : GC.AllocateUninitializedArray<DataCenterRawAttribute>(attrCount);

                for (var i = 0; i < attrCount; i++)
                {
                    var (index, value) = sortedAttributes[i];
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

                    rawAttributes[i] = new()
                    {
                        NameIndex = (ushort)index,
                        TypeInfo = (ushort)(ext << 2 | code),
                        Value = result,
                    };
                }

                if (!TryDeduplicateItems(_attributes, _attributeCache, rawAttributes, out attrAddr))
                {
                    attrAddr = AllocateRange(_attributes, attrCount, "Attribute");

                    WriteRange(_attributes, _attributeCache, rawAttributes, attrAddr);
                }
            }
            else
            {
                attrCount = 0;
                attrAddr = DataCenterAddress.MaxValue;
            }

            int childCount;
            DataCenterAddress childAddr;

            if (node.HasChildren)
            {
                var nodeChildren = node.UnsafeChildren;

                childCount = nodeChildren.Count;

                var sortedChildren = new (int Position, DataCenterNode Node)[childCount];

                for (var i = 0; i < childCount; i++)
                    sortedChildren[i] = (i, nodeChildren[i]);

                Array.Sort(sortedChildren, comparison);

                var rawChildren = childCount <= StackBufferSize
                    ? stackalloc DataCenterRawNode[childCount]
                    : GC.AllocateUninitializedArray<DataCenterRawNode>(childCount);

                for (var i = 0; i < childCount; i++)
                    rawChildren[i] = WriteTree(sortedChildren[i].Node);

                if (!TryDeduplicateItems(_nodes, _nodeCache, rawChildren, out childAddr))
                {
                    childAddr = AllocateRange(_nodes, childCount, "Node");

                    WriteRange(_nodes, _nodeCache, rawChildren, childAddr);
                }
            }
            else
            {
                childCount = 0;
                childAddr = DataCenterAddress.MaxValue;
            }

            var keys = node.Keys;

            return new()
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
            };
        }

        // The tree needs to be sorted according to the index of name strings. So we must walk the entire tree and
        // ensure that all names have been added to the table before we actually write the tree.
        DataCenterNameTree.Collect(root, s => _names.AddString(s), cancellationToken);

        _nodes.SetElement(AllocateRange(_nodes, 1, "Node"), WriteTree(root));
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
                var arch = _options.Format == DataCenterFormat.V6X64
                    ? DataCenterArchitecture.X64
                    : DataCenterArchitecture.X86;

                await _header.WriteAsync(writer, _options.Format, cancellationToken).ConfigureAwait(false);
                await _keys.WriteAsync(arch, writer, cancellationToken).ConfigureAwait(false);
                await _attributes.WriteAsync(arch, writer, cancellationToken).ConfigureAwait(false);
                await _nodes.WriteAsync(arch, writer, cancellationToken).ConfigureAwait(false);
                await _values.WriteAsync(arch, writer, cancellationToken).ConfigureAwait(false);
                await _names.WriteAsync(arch, writer, cancellationToken).ConfigureAwait(false);
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
