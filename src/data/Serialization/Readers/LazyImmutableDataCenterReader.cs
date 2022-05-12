using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

sealed class LazyImmutableDataCenterReader : DataCenterReader
{
    readonly ConcurrentDictionary<DataCenterAddress, LazyImmutableDataCenterNode> _cache = new();

    public LazyImmutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override LazyImmutableDataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys)
    {
        // This may result in redundant node allocations, but that has no side effects anyway, and only one wins.
        return _cache.GetOrAdd(
            address,
            _ =>
            {
                LazyImmutableDataCenterNode node = null!;

                return node = new LazyImmutableDataCenterNode(
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
                                throw new InvalidDataException(
                                    $"Attribute named '{name}' was already recorded earlier.");
                        });

                        return dict;
                    },
                    () =>
                    {
                        var list = new List<DataCenterNode>(raw.ChildCount);

                        ForEachChild(raw, node, list, static (list, node) => list.Add(node));

                        return list;
                    });
            });
    }

    protected override LazyImmutableDataCenterNode? ResolveNode(DataCenterAddress address, object parent)
    {
        return _cache.GetValueOrDefault(address) ?? Unsafe.As<LazyImmutableDataCenterNode>(CreateNode(address, parent));
    }
}
