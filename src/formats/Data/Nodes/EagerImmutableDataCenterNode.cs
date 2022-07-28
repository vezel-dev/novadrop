namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal sealed class EagerImmutableDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _attributes!;

    public override IReadOnlyList<DataCenterNode> Children => _children!;

    private IReadOnlyDictionary<string, DataCenterValue>? _attributes;

    private IReadOnlyList<DataCenterNode>? _children;

    public EagerImmutableDataCenterNode(DataCenterNode? parent, string name, string? value, DataCenterKeys keys)
        : base(parent, name, value, keys)
    {
    }

    public void Initialize(OrderedDictionary<string, DataCenterValue> attributes, List<DataCenterNode> children)
    {
        _attributes = attributes;
        _children = children;
    }
}
