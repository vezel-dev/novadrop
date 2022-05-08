namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class TransientDataCenterNode : ImmutableDataCenterNode
{
    public override IReadOnlyDictionary<string, DataCenterValue> Attributes => _getAttributes();

    public override IReadOnlyCollection<DataCenterNode> Children => _getChildren();

    readonly Func<IReadOnlyDictionary<string, DataCenterValue>> _getAttributes;

    readonly Func<IReadOnlyCollection<DataCenterNode>> _getChildren;

    public TransientDataCenterNode(
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys,
        Func<IReadOnlyDictionary<string, DataCenterValue>> getAttributes,
        Func<IReadOnlyCollection<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        _getAttributes = getAttributes;
        _getChildren = getChildren;
    }
}
