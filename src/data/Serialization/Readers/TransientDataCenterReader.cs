using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

sealed class TransientDataCenterReader : DataCenterReader
{
    public TransientDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override DataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        object parent,
        string name,
        string? value,
        DataCenterKeys keys)
    {
        TransientDataCenterNode node = null!;

        return node = new TransientDataCenterNode(
            parent,
            name,
            value,
            keys,
            (raw.AttributeCount - (value != null ? 1 : 0)) != 0,
            raw.ChildCount != 0,
            () =>
            {
                var dict = new OrderedDictionary<string, DataCenterValue>(raw.AttributeCount);

                ForEachAttribute(raw, dict, static (dict, name, value) =>
                {
                    if (!dict.TryAdd(name, value))
                        throw new InvalidDataException($"Attribute named '{name}' was already recorded earlier.");
                });

                return dict;
            },
            () =>
            {
                var list = new List<DataCenterNode>(raw.ChildCount);

                ForEachChild(raw, node, list, static (list, node) => list.Add(node));

                return list;
            });
    }

    protected override DataCenterNode? ResolveNode(DataCenterAddress address, object parent)
    {
        return CreateNode(address, parent);
    }
}
