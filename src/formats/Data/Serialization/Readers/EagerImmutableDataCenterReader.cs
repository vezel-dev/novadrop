using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

internal sealed class EagerImmutableDataCenterReader : DataCenterReader
{
    private static readonly OrderedDictionary<string, DataCenterValue> _emptyAttributes = new();

    private static readonly List<DataCenterNode> _emptyChildren = new();

    private readonly Dictionary<DataCenterAddress, EagerImmutableDataCenterNode> _cache = new();

    public EagerImmutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override EagerImmutableDataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken)
    {
        var node = new EagerImmutableDataCenterNode(parent, name, value, keys);

        _cache.Add(address, node);

        var attributes = _emptyAttributes;
        var attrCount = raw.AttributeCount - (value != null ? 1 : 0);

        if (attrCount != 0)
        {
            attributes = new(attrCount);

            ReadAttributes(
                raw,
                attributes,
                static (attributes, name, value) =>
                    Check.Data(
                        attributes.TryAdd(name, value), $"Attribute named '{name}' was already recorded earlier."));
        }

        var children = _emptyChildren;

        if (raw.ChildCount != 0)
        {
            children = new(raw.ChildCount);

            ReadChildren(raw, node, children, static (children, node) => children.Add(node), cancellationToken);
        }

        node.Initialize(attributes, children);

        return node;
    }

    protected override EagerImmutableDataCenterNode? ResolveNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken)
    {
        return _cache.GetValueOrDefault(address) ??
            Unsafe.As<EagerImmutableDataCenterNode>(CreateNode(address, parent, cancellationToken));
    }
}
