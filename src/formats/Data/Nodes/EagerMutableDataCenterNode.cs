namespace Vezel.Novadrop.Data.Nodes;

internal sealed class EagerMutableDataCenterNode : MutableDataCenterNode
{
    public override OrderedDictionary<string, DataCenterValue> Attributes => _attributes ??= [];

    public override List<DataCenterNode> Children => _children ??= [];

    private OrderedDictionary<string, DataCenterValue>? _attributes;

    private List<DataCenterNode>? _children;

    public EagerMutableDataCenterNode(
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        int attributeCount,
        int childCount)
        : base(parent, name, value, keys)
    {
        if (attributeCount != 0)
            _attributes = new(attributeCount);

        if (childCount != 0)
            _children = new(childCount);
    }
}
