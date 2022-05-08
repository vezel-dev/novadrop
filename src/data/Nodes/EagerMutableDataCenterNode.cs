namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class EagerMutableDataCenterNode : MutableDataCenterNode
{
    public override Dictionary<string, DataCenterValue> Attributes { get; }

    public override List<DataCenterNode> Children { get; } = new();

    public EagerMutableDataCenterNode(
        object parent,
        string name,
        DataCenterKeys keys,
        int attributeCount,
        int childCount)
        : base(parent, name, keys)
    {
        Attributes = new(attributeCount);
        Children = new(childCount);
    }
}
