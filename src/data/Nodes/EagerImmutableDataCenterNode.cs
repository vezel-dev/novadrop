namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class EagerImmutableDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _attributes!;

    public override IReadOnlyCollection<DataCenterNode> Children => _children!;

    IReadOnlyDictionary<string, DataCenterValue>? _attributes;

    IReadOnlyCollection<DataCenterNode>? _children;

    public EagerImmutableDataCenterNode(object parent, string name, DataCenterKeys keys)
        : base(parent, name, keys)
    {
    }

    public void Initialize(Dictionary<string, DataCenterValue> attributes, List<DataCenterNode> children)
    {
        _attributes = attributes;
        _children = children;
    }
}
