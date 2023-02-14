using Vezel.Novadrop.Cryptography;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;
using Vezel.Novadrop.Data.Serialization.Tables;

namespace Vezel.Novadrop.Data.Serialization.Readers;

internal abstract class DataCenterReader
{
    private readonly DataCenterHeader _header = new();

    private readonly DataCenterKeysTableReader _keys;

    private readonly DataCenterSegmentedRegion<DataCenterRawAttribute> _attributes = new();

    private readonly DataCenterSegmentedRegion<DataCenterRawNode> _nodes = new();

    private readonly DataCenterStringTableReader _values = new(DataCenterConstants.ValueTableSize);

    private readonly DataCenterStringTableReader _names = new(DataCenterConstants.NameTableSize);

    private readonly DataCenterFooter _footer = new();

    private readonly DataCenterLoadOptions _options;

    protected DataCenterReader(DataCenterLoadOptions options)
    {
        _keys = new(_names);
        _options = options;
    }

    protected abstract DataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken);

    protected abstract DataCenterNode? ResolveNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken);

    protected void ReadAttributes<T>(DataCenterRawNode raw, T state, Action<T, string, DataCenterValue> action)
    {
        var addr = raw.AttributeAddress;

        for (var i = 0; i < raw.AttributeCount; i++)
        {
            var (name, value) = CreateAttribute(
                new DataCenterAddress(addr.SegmentIndex, (ushort)(addr.ElementIndex + i)));

            // Node value attributes are handled in CreateNode.
            if (name != DataCenterConstants.ValueAttributeName)
                action(state, name, value);
            else
                Check.Data(i == raw.AttributeCount - 1, $"Special '{name}' attribute is not sorted last.");
        }
    }

    protected void ReadChildren<T>(
        DataCenterRawNode raw,
        DataCenterNode? parent,
        T state,
        Action<T, DataCenterNode> action,
        CancellationToken cancellationToken)
    {
        var addr = raw.ChildAddress;

        for (var i = 0; i < raw.ChildCount; i++)
        {
            var child = ResolveNode(new(addr.SegmentIndex, (ushort)(addr.ElementIndex + i)), parent, cancellationToken);

            // Discard empty nodes.
            if (child != null)
                action(state, child);
        }
    }

    private (string Name, DataCenterValue Value) CreateAttribute(DataCenterAddress address)
    {
        var rawAttr = _attributes.GetElement(address);

        var typeCode = rawAttr.TypeInfo & 0b0000000000000011;
        var extCode = (rawAttr.TypeInfo & 0b1111111111111100) >> 2;

        var result = (typeCode, extCode, rawAttr.Value) switch
        {
            (1, 0, var value) => value,
            (1, 1, 0) => false,
            (1, 1, not 1 and var value) when _options.Strict =>
                throw new InvalidDataException($"Attribute has invalid Boolean value {value}."),
            (1, 1, _) => true,
            (2, not 0, _) when _options.Strict =>
                throw new InvalidDataException($"Attribute has invalid extended type code {extCode}."),
            (2, _, var value) => Unsafe.As<int, float>(ref value),
            (3, _, _) => default(DataCenterValue), // Handled below.
            _ => throw new InvalidDataException($"Attribute has invalid type code {typeCode}."),
        };

        // String addresses need some extra work to handle endianness properly.
        if (result.IsNull)
        {
            var segIdx = (ushort)rawAttr.Value;
            var elemIdx = (ushort)((rawAttr.Value & 0b11111111111111110000000000000000) >> 16);

            if (!BitConverter.IsLittleEndian)
            {
                segIdx = BinaryPrimitives.ReverseEndianness(segIdx);
                elemIdx = BinaryPrimitives.ReverseEndianness(elemIdx);
            }

            var str = _values.GetString(new DataCenterAddress(segIdx, elemIdx));

            result = new(str);

            if (_options.Strict)
            {
                var hash = DataCenterHash.ComputeValueHash(str);

                Check.Data(extCode == hash, $"Value hash 0x{extCode:x8} does not match expected hash 0x{hash:x8}.");
            }
        }

        // Note: Padding1 is allowed to contain garbage. Do not check.

        return (_names.GetString(rawAttr.NameIndex - 1), result);
    }

    protected DataCenterNode? CreateNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var raw = _nodes.GetElement(address);

        var nameIdx = raw.NameIndex - 1;

        // Is this a placeholder node? If so, the rest of the contents are garbage. Discard it.
        if (nameIdx == -1)
            return null;

        var name = _names.GetString(nameIdx);
        var keysInfo = raw.KeysInfo;
        var keyFlags = keysInfo & 0b0000000000001111;
        var attrCount = raw.AttributeCount;
        var childCount = raw.ChildCount;
        var attrAddr = raw.AttributeAddress;
        var childAddr = raw.ChildAddress;

        if (_options.Strict)
        {
            if (name == DataCenterConstants.RootNodeName)
            {
                Check.Data(parent == null, $"Node name '{name}' is only valid for the root node.");
                Check.Data(attrCount == 0, $"Root node has {attrCount} attributes (expected 0).");
            }

            // TODO: Should we allow setting 0b0001 in the API?
            Check.Data(keyFlags is 0b0000 or 0b0001, $"Node has invalid key flags 0x{keyFlags:x1}.");

            var max = DataCenterAddress.MaxValue;

            Check.Data(
                attrAddr.ElementIndex + attrCount <= max.ElementIndex + 1,
                $"Cannot read {attrCount} contiguous attributes at {attrAddr}.");
            Check.Data(
                childAddr.ElementIndex + childCount <= max.ElementIndex + 1,
                $"Cannot read {childCount} contiguous nodes at {childAddr}.");
        }

        var value = default(string);

        // The node value attribute, if present, is always last.
        if (attrCount != 0)
        {
            var (attrName, attrValue) = CreateAttribute(
                new(attrAddr.SegmentIndex, (ushort)(attrAddr.ElementIndex + attrCount - 1)));

            if (attrName == DataCenterConstants.ValueAttributeName)
            {
                Check.Data(
                    attrValue.IsString,
                    $"Special '{attrName}' attribute has invalid type {attrValue.TypeCode} " +
                    $"(expected {DataCenterTypeCode.String}).");

                value = attrValue.UnsafeAsString;
            }
        }

        // Note: Padding1 and Padding2 are allowed to contain garbage. Do not check.

        return AllocateNode(
            address, raw, parent, name, value, _keys.GetKeys((keysInfo & 0b1111111111110000) >> 4), cancellationToken);
    }

    public Task<DataCenterNode> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        return Task.Run(
            async () =>
            {
                using var aes = DataCenter.CreateCipher(_options.Key, _options.IV);
                using var decryptor = aes.CreateDecryptor();
                using var padder = new FakePaddingCryptoTransform(decryptor);
                var cryptoStream = new CryptoStream(stream, padder, CryptoStreamMode.Read, true);

                await using (cryptoStream.ConfigureAwait(false))
                {
                    var size = await new StreamBinaryReader(cryptoStream)
                        .ReadUInt32Async(cancellationToken)
                        .ConfigureAwait(false);

                    var zlibStream = new ZLibStream(cryptoStream, CompressionMode.Decompress, true);

                    await using (zlibStream.ConfigureAwait(false))
                    {
                        var reader = new StreamBinaryReader(zlibStream);
                        var strict = _options.Strict;

                        await _header.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                        await _keys.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
                        await _attributes.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                        await _nodes.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                        await _values.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                        await _names.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                        await _footer.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);

                        Check.Data(
                            !strict || reader.Progress == size,
                            $"Uncompressed data center size {size} does not match actual size {reader.Progress}.");
                    }
                }

                var root = CreateNode(DataCenterAddress.MinValue, null, cancellationToken);

                Check.Data(root != null, $"Root node is empty.");
                Check.Data(
                    !_options.Strict || root.Name == DataCenterConstants.RootNodeName,
                    $"Root node name '{root.Name}' does not match expected '{DataCenterConstants.RootNodeName}'.");

                return root;
            },
            cancellationToken);
    }
}
