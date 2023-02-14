using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

internal sealed class TransientDataCenterReader : DataCenterReader
{
    private static readonly OrderedDictionary<string, DataCenterValue> _emptyAttributes = new();

    private static readonly List<DataCenterNode> _emptyChildren = new();

    public TransientDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override DataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken)
    {
        TransientDataCenterNode node = null!;

        var attrCount = raw.AttributeCount - (value != null ? 1 : 0);

        return node = new(
            parent,
            name,
            value,
            keys,
            attrCount != 0,
            raw.ChildCount != 0,
            () =>
            {
                var attributes = _emptyAttributes;

                if (node.HasAttributes)
                {
                    attributes = new(attrCount);

                    ReadAttributes(
                        raw,
                        attributes,
                        static (attributes, name, value) =>
                            Check.Data(
                                attributes.TryAdd(name, value),
                                $"Attribute named '{name}' was already recorded earlier."));
                }

                return attributes;
            },
            () =>
            {
                var children = _emptyChildren;

                if (node.HasChildren)
                {
                    children = new(raw.ChildCount);

                    ReadChildren(raw, node, children, static (children, node) => children.Add(node), default);
                }

                return children;
            });
    }

    protected override DataCenterNode? ResolveNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken)
    {
        return CreateNode(address, parent, default);
    }
}
