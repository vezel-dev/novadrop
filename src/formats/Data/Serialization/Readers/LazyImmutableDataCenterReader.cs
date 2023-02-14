using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

internal sealed class LazyImmutableDataCenterReader : DataCenterReader
{
    private static readonly OrderedDictionary<string, DataCenterValue> _emptyAttributes = new();

    private static readonly List<DataCenterNode> _emptyChildren = new();

    private readonly ConcurrentDictionary<DataCenterAddress, LazyImmutableDataCenterNode> _cache = new();

    public LazyImmutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override LazyImmutableDataCenterNode AllocateNode(
        DataCenterAddress address,
        DataCenterRawNode raw,
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        CancellationToken cancellationToken)
    {
        // This may result in redundant node allocations, but that has no side effects anyway, and only one wins.
        return _cache.GetOrAdd(
            address,
            _ =>
            {
                LazyImmutableDataCenterNode node = null!;

                return node = new(
                    parent,
                    name,
                    value,
                    keys,
                    () =>
                    {
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
                                        attributes.TryAdd(name, value),
                                        $"Attribute named '{name}' was already recorded earlier."));
                        }

                        return attributes;
                    },
                    () =>
                    {
                        var children = _emptyChildren;

                        if (raw.ChildCount != 0)
                        {
                            children = new(raw.ChildCount);

                            ReadChildren(raw, node, children, static (children, node) => children.Add(node), default);
                        }

                        return children;
                    });
            });
    }

    protected override LazyImmutableDataCenterNode? ResolveNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken)
    {
        return _cache.GetValueOrDefault(address) ??
            Unsafe.As<LazyImmutableDataCenterNode>(CreateNode(address, parent, default));
    }
}
