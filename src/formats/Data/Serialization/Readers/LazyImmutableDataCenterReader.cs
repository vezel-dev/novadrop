using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

internal sealed class LazyImmutableDataCenterReader : DataCenterReader
{
    private static readonly OrderedDictionary<string, DataCenterValue> _emptyAttributes = [];

    private static readonly List<DataCenterNode> _emptyChildren = [];

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
            static (_, tup) =>
            {
                var @this = tup.This;
                var raw = tup.Raw;

                LazyImmutableDataCenterNode node = null!;

                return node = new(
                    tup.Parent,
                    tup.Name,
                    tup.Value,
                    tup.Keys,
                    () =>
                    {
                        var attributes = _emptyAttributes;
                        var attrCount = raw.AttributeCount - (@tup.Value != null ? 1 : 0);

                        if (attrCount != 0)
                        {
                            attributes = new(attrCount);

                            @this.ReadAttributes(
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

                            @this.ReadChildren(
                                raw,
                                node,
                                children,
                                static (children, node) => children.Add(node),
                                CancellationToken.None);
                        }

                        return children;
                    });
            },
            (This: this, Raw: raw, Parent: parent, Name: name, Value: value, Keys: keys));
    }

    protected override LazyImmutableDataCenterNode? ResolveNode(
        DataCenterAddress address, DataCenterNode? parent, CancellationToken cancellationToken)
    {
        return _cache.GetValueOrDefault(address) ??
            Unsafe.As<LazyImmutableDataCenterNode>(CreateNode(address, parent, CancellationToken.None));
    }
}
