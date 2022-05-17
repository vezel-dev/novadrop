using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

sealed class DataCenterKeysTableWriter
{
    readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(false);

    readonly Dictionary<(string?, string?, string?, string?), int> _cache = new(ushort.MaxValue);

    readonly DataCenterStringTableWriter _names;

    public DataCenterKeysTableWriter(DataCenterStringTableWriter names)
    {
        _names = names;
    }

    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await _keys.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
    }

    public int AddKeys(string? attributeName1, string? attributeName2, string? attributeName3, string? attributeName4)
    {
        var tup = (attributeName1, attributeName2, attributeName3, attributeName4);

        if (!_cache.TryGetValue(tup, out var index))
        {
            if (_keys.Elements.Count == DataCenterConstants.KeysTableSize)
                throw new InvalidOperationException(
                    $"Keys table is full ({DataCenterConstants.KeysTableSize} elements).");

            ushort GetIndex(string? value)
            {
                return (ushort)(value != null ? _names.AddString(value).Index : 0);
            }

            _keys.Elements.Add(new DataCenterRawKeys
            {
                NameIndex1 = GetIndex(attributeName1),
                NameIndex2 = GetIndex(attributeName2),
                NameIndex3 = GetIndex(attributeName3),
                NameIndex4 = GetIndex(attributeName4),
            });

            index = _keys.Elements.Count - 1;

            _cache.Add(tup, index);
        }

        return index;
    }
}
