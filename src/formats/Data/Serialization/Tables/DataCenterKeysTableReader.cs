// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

internal sealed class DataCenterKeysTableReader
{
    private readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(offByOne: false);

    private readonly List<DataCenterKeys> _byIndex = new(ushort.MaxValue);

    private readonly DataCenterStringTableReader _names;

    public DataCenterKeysTableReader(DataCenterStringTableReader names)
    {
        _names = names;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(
        DataCenterArchitecture architecture, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        await _keys.ReadAsync(architecture, reader, cancellationToken).ConfigureAwait(false);
    }

    public void Populate()
    {
        // This has to happen after the names string table has been read.

        foreach (var raw in _keys.Elements)
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

            _byIndex.Add(new(
                GetName(raw.NameIndex1), GetName(raw.NameIndex2), GetName(raw.NameIndex3), GetName(raw.NameIndex4)));
        }
    }

    public DataCenterKeys GetKeys(int index)
    {
        Check.Data(index < _byIndex.Count, $"Keys table index {index} is out of bounds (0..{_byIndex.Count}).");

        return _byIndex[index];
    }
}
