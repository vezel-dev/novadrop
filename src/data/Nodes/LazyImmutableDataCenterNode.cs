namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class LazyImmutableDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _attributes!.Value;

    public override IReadOnlyCollection<DataCenterNode> Children => _children!.Value;

    readonly Lazy<IReadOnlyDictionary<string, DataCenterValue>>? _attributes;

    readonly Lazy<IReadOnlyCollection<DataCenterNode>>? _children;

    public LazyImmutableDataCenterNode(
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys,
        Func<IReadOnlyDictionary<string, DataCenterValue>> getAttributes,
        Func<IReadOnlyCollection<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        _attributes = new(getAttributes, false);
        _children = new(getChildren, false);
    }
}
