namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class EagerMutableDataCenterNode : MutableDataCenterNode
{
    public override Dictionary<string, DataCenterValue> Attributes { get; }

    public override List<DataCenterNode> Children { get; } = new();

    public EagerMutableDataCenterNode(
        object parent,
        string name,
        DataCenterValue value,
        DataCenterKeys keys,
        int attributeCount,
        int childCount)
        : base(parent, name, value, keys)
    {
        Attributes = new(attributeCount);
        Children = new(childCount);
    }
}
