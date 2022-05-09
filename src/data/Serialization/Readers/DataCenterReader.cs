using Vezel.Novadrop.Data.IO;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;
using Vezel.Novadrop.Data.Serialization.Tables;

namespace Vezel.Novadrop.Data.Serialization.Readers;

abstract class DataCenterReader
{
    readonly DataCenterHeader _header = new();

    readonly DataCenterKeysTableReader _keys;

    readonly DataCenterSegmentedRegion<DataCenterRawAttribute> _attributes = new();

    readonly DataCenterSegmentedRegion<DataCenterRawNode> _nodes = new();

    readonly DataCenterStringTableReader _values = new(DataCenterConstants.ValueTableSize);

    readonly DataCenterStringTableReader _names = new(DataCenterConstants.NameTableSize);

    readonly DataCenterFooter _footer = new();

    readonly DataCenterLoadOptions _options;

    protected DataCenterReader(DataCenterLoadOptions options)
    {
        _keys = new(_names);
        _options = options;
    }

    protected abstract DataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys);

    protected abstract DataCenterNode? ResolveNode(DataCenterAddress address, object parent);

    protected void ForEachAttribute<T>(DataCenterRawNode raw, T state, Action<T, string, DataCenterValue> action)
    {
        var addr = raw.AttributeAddress;

        for (var i = 0; i < raw.AttributeCount; i++)
        {
            var (name, value) = CreateAttribute(
                new DataCenterAddress(addr.SegmentIndex, (ushort)(addr.ElementIndex + i)));

            // Node value attributes are handled in CreateNode.
            if (name != DataCenterConstants.ValueAttributeName)
                action(state, name, value);
        }
    }

    protected void ForEachChild<T>(DataCenterRawNode raw, object parent, T state, Action<T, DataCenterNode> action)
    {
        var addr = raw.ChildAddress;

        for (var i = 0; i < raw.ChildCount; i++)
        {
            var child = ResolveNode(new DataCenterAddress(addr.SegmentIndex, (ushort)(addr.ElementIndex + i)), parent);

            // Discard empty nodes.
            if (child != null)
                action(state, child);
        }
    }

    (string Name, DataCenterValue Value) CreateAttribute(DataCenterAddress address)
    {
        var rawAttr = _attributes.GetElement(address);

        var typeCode = rawAttr.TypeInfo & 0b0000000000000011;
        var extCode = (rawAttr.TypeInfo & 0b1111111111111100) >> 2;

        var result = typeCode switch
        {
            1 => extCode switch
            {
                0 => rawAttr.Value,
                1 => rawAttr.Value switch
                {
                    0 => false,
                    1 => true,
                    var v when _options.Strict =>
                        throw new InvalidDataException($"Attribute has invalid Boolean value {v}."),
                    _ => true,
                },
                var e => throw new InvalidDataException($"Attribute has invalid extended type code {e}."),
            },
            2 => extCode switch
            {
                not 0 and var e when _options.Strict =>
                    throw new InvalidDataException($"Attribute has invalid extended type code {e}."),
                _ => Unsafe.As<int, float>(ref rawAttr.Value),
            },
            3 => default(DataCenterValue), // Handled below.
            var t => throw new InvalidDataException($"Attribute has invalid type code {t}."),
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

                if (extCode != hash)
                    throw new InvalidDataException(
                        $"Value hash 0x{extCode:x8} does not match expected hash 0x{hash:x8}.");
            }
        }

        // Note: Padding1 is allowed to contain garbage. Do not check.

        return (_names.GetString(rawAttr.NameIndex - 1), result);
    }

    protected DataCenterNode? CreateNode(DataCenterAddress address, object parent)
    {
        var raw = _nodes.GetElement(address);

        var nameIdx = raw.NameIndex - 1;

        // Is this a placeholder node? If so, the rest of the contents are garbage. Discard it.
        if (nameIdx == -1)
            return null;

        var name = _names.GetString(nameIdx);

        if (_options.Strict && name == DataCenterConstants.RootNodeName && parent is not DataCenter)
            throw new InvalidDataException($"Node name '{name}' is only valid for the root node.");

        var keysInfo = raw.KeysInfo;
        var keyFlags = keysInfo & 0b0000000000001111;

        // TODO: Should we allow setting 0b0001 in the API?
        if (_options.Strict && keyFlags is not 0b0000 or 0b0001)
            throw new InvalidDataException($"Node has invalid key flags 0x{keyFlags:x1}.");

        var max = DataCenterAddress.MaxValue;
        var attrCount = raw.AttributeCount;
        var attrAddr = raw.AttributeAddress;

        if (_options.Strict && attrAddr.ElementIndex + attrCount > max.ElementIndex + 1)
            throw new InvalidDataException($"Cannot read {attrCount} contiguous attributes at {attrAddr}.");

        var childCount = raw.ChildCount;
        var childAddr = raw.ChildAddress;

        if (_options.Strict && childAddr.ElementIndex + childCount > max.ElementIndex + 1)
            throw new InvalidDataException($"Cannot read {childCount} contiguous nodes at {childAddr}.");

        // Note: Padding1 and Padding2 are allowed to contain garbage. Do not check.

        var value = default(DataCenterValue);

        // The node value attribute, if present, is always last.
        if (attrCount != 0)
        {
            var (attrName, attrValue) = CreateAttribute(
                new(attrAddr.SegmentIndex, (ushort)(attrAddr.ElementIndex + attrCount - 1)));

            if (attrName == DataCenterConstants.ValueAttributeName)
                value = attrValue;
        }

        return AllocateNode(address, raw, parent, name, value, _keys.GetKeys((keysInfo & 0b1111111111110000) >> 4));
    }

    public async Task<DataCenterNode> ReadAsync(Stream stream, DataCenter center, CancellationToken cancellationToken)
    {
        await Task.Yield();

        using var aes = DataCenter.CreateCipher(_options.Key, _options.IV);
        using var decryptor = aes.CreateDecryptor();
        var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true);

        await using (cryptoStream.ConfigureAwait(false))
        {
            var size = await new DataCenterBinaryReader(cryptoStream)
                .ReadUInt32Async(cancellationToken)
                .ConfigureAwait(false);

            var zlibStream = new ZLibStream(cryptoStream, CompressionMode.Decompress, true);

            await using (zlibStream.ConfigureAwait(false))
            {
                var reader = new DataCenterBinaryReader(zlibStream);
                var strict = _options.Strict;

                await _header.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                await _keys.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
                await _attributes.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                await _nodes.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                await _values.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                await _names.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);
                await _footer.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);

                if (strict && reader.Progress != size)
                    throw new InvalidDataException(
                        $"Uncompressed data center size {size} does not match actual size {reader.Progress}.");
            }
        }

        var root = CreateNode(DataCenterAddress.MinValue, center);

        return root != null
            ? _options.Strict && root.Name == DataCenterConstants.RootNodeName
                ? root
                : throw new InvalidDataException(
                    $"Root node name '{root.Name}' does not match expected '{DataCenterConstants.RootNodeName}'.")
            : throw new InvalidDataException("Root node is empty.");
    }
}
