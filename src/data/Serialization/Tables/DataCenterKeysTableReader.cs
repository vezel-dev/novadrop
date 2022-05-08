using Vezel.Novadrop.Data.IO;
using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

sealed class DataCenterKeysTableReader
{
    readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(false);

    readonly List<DataCenterKeys?> _cache = new(ushort.MaxValue);

    readonly DataCenterStringTableReader _names;

    public DataCenterKeysTableReader(DataCenterStringTableReader names)
    {
        _names = names;
    }

    public async ValueTask ReadAsync(DataCenterBinaryReader reader, CancellationToken cancellationToken)
    {
        await _keys.ReadAsync(reader, cancellationToken).ConfigureAwait(false);

        for (var i = 0; i < _keys.Elements.Count; i++)
            _cache.Add(null);
    }

    public DataCenterKeys GetKeys(int index)
    {
        if (index >= _keys.Elements.Count)
            throw new InvalidDataException($"Keys table index {index} is out of bounds (0..{_keys.Elements.Count}).");

        string? GetName(int index)
        {
            var nameIdx = index - 1;

            if (nameIdx == -1)
                return null;

            var name = _names.GetString(nameIdx);

            return name != DataCenterConstants.ValueAttributeName
                ? name
                : throw new InvalidDataException($"Key entry refers to illegal attribute name '{name}'.");
        }

        if (_cache[index] is DataCenterKeys keys)
            return keys;

        var raw = _keys.Elements[index];

        return _cache[index] =
            new(GetName(raw.NameIndex1), GetName(raw.NameIndex2), GetName(raw.NameIndex3), GetName(raw.NameIndex4));
    }
}
