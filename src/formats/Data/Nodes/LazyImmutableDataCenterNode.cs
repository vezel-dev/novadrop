namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal sealed class LazyImmutableDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _attributes!.Value;

    public override IReadOnlyList<DataCenterNode> Children => _children!.Value;

    private readonly Lazy<IReadOnlyDictionary<string, DataCenterValue>>? _attributes;

    private readonly Lazy<IReadOnlyList<DataCenterNode>>? _children;

    public LazyImmutableDataCenterNode(
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        Func<IReadOnlyDictionary<string, DataCenterValue>> getAttributes,
        Func<IReadOnlyList<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        _attributes = new(getAttributes);
        _children = new(getChildren);
    }
}
