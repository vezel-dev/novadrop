// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Data.Serialization.Items;
using Vezel.Novadrop.Data.Serialization.Regions;

namespace Vezel.Novadrop.Data.Serialization.Tables;

internal sealed class DataCenterKeysTableWriter
{
    private readonly DataCenterSimpleRegion<DataCenterRawKeys> _keys = new(offByOne: false);

    private readonly Dictionary<(string?, string?, string?, string?), int> _indexes = new(ushort.MaxValue);

    private readonly DataCenterStringTableWriter _names;

    public DataCenterKeysTableWriter(DataCenterStringTableWriter names)
    {
        _names = names;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(
        DataCenterArchitecture architecture, StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await _keys.WriteAsync(architecture, writer, cancellationToken).ConfigureAwait(false);
    }

    public int AddKeys(string? attributeName1, string? attributeName2, string? attributeName3, string? attributeName4)
    {
        var tup = (attributeName1, attributeName2, attributeName3, attributeName4);

        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault(_indexes, tup, out var exists);

        if (!exists)
        {
            Check.Operation(
                _keys.Elements.Count != DataCenterConstants.KeysTableSize,
                $"Keys table is full ({DataCenterConstants.KeysTableSize} elements).");

            ushort GetIndex(string? value)
            {
                return (ushort)(value != null ? _names.AddString(value).Index : 0);
            }

            _keys.Elements.Add(new()
            {
                NameIndex1 = GetIndex(attributeName1),
                NameIndex2 = GetIndex(attributeName2),
                NameIndex3 = GetIndex(attributeName3),
                NameIndex4 = GetIndex(attributeName4),
            });

            index = _keys.Elements.Count - 1;
        }

        return index;
    }
}
