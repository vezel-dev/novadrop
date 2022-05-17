using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

sealed class DataCenterKeysTableReader
{
    readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(false);

    readonly ConcurrentDictionary<int, DataCenterKeys> _cache = new();

    readonly DataCenterStringTableReader _names;

    public DataCenterKeysTableReader(DataCenterStringTableReader names)
    {
        _names = names;
    }

    public async ValueTask ReadAsync(StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        await _keys.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    public DataCenterKeys GetKeys(int index)
    {
        return index < _keys.Elements.Count
            ? _cache.GetOrAdd(
                index,
                i =>
                {
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

                    var raw = _keys.Elements[i];

                    return new(
                        GetName(raw.NameIndex1),
                        GetName(raw.NameIndex2),
                        GetName(raw.NameIndex3),
                        GetName(raw.NameIndex4));
                })
            : throw new InvalidDataException($"Keys table index {index} is out of bounds (0..{_keys.Elements.Count}).");
    }
}
