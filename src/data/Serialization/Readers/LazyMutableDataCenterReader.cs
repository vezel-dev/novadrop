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
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken)
    {
        LazyMutableDataCenterNode node = null!;

        return node = new LazyMutableDataCenterNode(
            parent,
            name,
            value,
            keys,
            () =>
            {
                var dict = new OrderedDictionary<string, DataCenterValue>(raw.AttributeCount);

                ReadAttributes(raw, dict, static (dict, name, value) =>
                {
                    if (!dict.TryAdd(name, value))
                        throw new InvalidDataException($"Attribute named '{name}' was already recorded earlier.");
                });

                return dict;
            },
            () =>
            {
                var list = new List<DataCenterNode>(raw.ChildCount);

                ReadChildren(raw, node, list, static (list, node) => list.Add(node), default);

                return list;
            });
    }

    protected override LazyImmutableDataCenterNode? ResolveNode(
        DataCenterAddress address, object parent, CancellationToken cancellationToken)
    {
        return Unsafe.As<LazyImmutableDataCenterNode>(CreateNode(address, parent, default));
    }
}
