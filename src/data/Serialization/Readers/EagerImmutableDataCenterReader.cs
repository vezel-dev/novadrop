using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

sealed class EagerImmutableDataCenterReader : DataCenterReader
{
    readonly Dictionary<DataCenterAddress, EagerImmutableDataCenterNode> _cache = new();

    public EagerImmutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override EagerImmutableDataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        object parent,
        string name,
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken)
    {
        var node = new EagerImmutableDataCenterNode(parent, name, value, keys);

        _cache.Add(address, node);

        var dict = new OrderedDictionary<string, DataCenterValue>(raw.AttributeCount);

        ReadAttributes(raw, dict, static (dict, name, value) =>
        {
            if (!dict.TryAdd(name, value))
                throw new InvalidDataException($"Attribute named '{name}' was already recorded earlier.");
        });

        var list = new List<DataCenterNode>(raw.ChildCount);

        ReadChildren(raw, node, list, static (list, node) => list.Add(node), cancellationToken);

        node.Initialize(dict, list);

        return node;
    }

    protected override EagerImmutableDataCenterNode? ResolveNode(
        DataCenterAddress address, object parent, CancellationToken cancellationToken)
    {
        return _cache.GetValueOrDefault(address) ??
            Unsafe.As<EagerImmutableDataCenterNode>(CreateNode(address, parent, cancellationToken));
    }
}
