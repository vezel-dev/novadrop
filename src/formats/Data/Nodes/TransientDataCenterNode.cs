namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal sealed class TransientDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _getAttributes();

    public override bool HasAttributes { get; }

    public override IReadOnlyList<DataCenterNode> Children => _getChildren();

    public override bool HasChildren { get; }

    private readonly Func<IReadOnlyDictionary<string, DataCenterValue>> _getAttributes;

    private readonly Func<IReadOnlyList<DataCenterNode>> _getChildren;

    public TransientDataCenterNode(
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        bool hasAttributes,
        bool hasChildren,
        Func<IReadOnlyDictionary<string, DataCenterValue>> getAttributes,
        Func<IReadOnlyList<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        HasAttributes = hasAttributes;
        HasChildren = hasChildren;
        _getAttributes = getAttributes;
        _getChildren = getChildren;
    }
}
