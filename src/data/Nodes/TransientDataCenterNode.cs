namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class TransientDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _getAttributes();

    public override bool HasAttributes { get; }

    public override IReadOnlyCollection<DataCenterNode> Children => _getChildren();

    public override bool HasChildren { get; }

    readonly Func<IReadOnlyDictionary<string, DataCenterValue>> _getAttributes;

    readonly Func<IReadOnlyCollection<DataCenterNode>> _getChildren;

    public TransientDataCenterNode(
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys,
        bool hasAttributes,
        bool hasChildren,
        Func<IReadOnlyDictionary<string, DataCenterValue>> getAttributes,
        Func<IReadOnlyCollection<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        HasAttributes = hasAttributes;
        HasChildren = hasChildren;
        _getAttributes = getAttributes;
        _getChildren = getChildren;
    }
}
