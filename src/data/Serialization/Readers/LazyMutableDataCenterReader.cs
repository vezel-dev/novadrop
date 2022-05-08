using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

sealed class LazyMutableDataCenterReader : DataCenterReader
{
    public LazyMutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override LazyMutableDataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys)
    {
        LazyMutableDataCenterNode node = null!;

        return node = new LazyMutableDataCenterNode(
            parent,
            name,
            value,
            keys,
            () =>
            {
                var dict = new Dictionary<string, DataCenterValue>(raw.AttributeCount);

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

    protected override LazyImmutableDataCenterNode? ResolveNode(DataCenterAddress address, object parent)
    {
        return Unsafe.As<LazyImmutableDataCenterNode>(CreateNode(address, parent));
    }
}
