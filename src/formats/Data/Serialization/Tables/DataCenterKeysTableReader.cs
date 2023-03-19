using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

internal sealed class DataCenterKeysTableReader
{
    private readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(false);

    private readonly ConcurrentDictionary<int, DataCenterKeys> _cache = new();

    private readonly DataCenterStringTableReader _names;

    public DataCenterKeysTableReader(DataCenterStringTableReader names)
    {
        _names = names;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        await _keys.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    public DataCenterKeys GetKeys(int index)
    {
        Check.Data(
            index < _keys.Elements.Count, $"Keys table index {index} is out of bounds (0..{_keys.Elements.Count}).");

        return _cache.GetOrAdd(
            index,
            i =>
            {
                string? GetName(int index)
                {
                    var nameIdx = index - 1;

                    if (nameIdx == -1)
                        return null;

                    var name = _names.GetString(nameIdx);

                    Check.Data(
                        name != DataCenterConstants.ValueAttributeName,
                        $"Key entry refers to illegal attribute name '{name}'.");

                    return name;
                }

                var raw = _keys.Elements[i];

                return new(
                    GetName(raw.NameIndex1),
                    GetName(raw.NameIndex2),
                    GetName(raw.NameIndex3),
                    GetName(raw.NameIndex4));
            });
    }
}
