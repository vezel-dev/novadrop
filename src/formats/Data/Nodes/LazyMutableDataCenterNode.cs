namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class LazyMutableDataCenterNode : MutableDataCenterNode
{
    public override OrderedDictionary<string, DataCenterValue> Attributes => _attributes!.Value;

    public override List<DataCenterNode> Children => _children!.Value;

    readonly Lazy<OrderedDictionary<string, DataCenterValue>>? _attributes;

    readonly Lazy<List<DataCenterNode>>? _children;

    public LazyMutableDataCenterNode(
        DataCenterNode? parent,
        string name,
        string? value,
        DataCenterKeys keys,
        Func<OrderedDictionary<string, DataCenterValue>> getAttributes,
        Func<List<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        _attributes = new(getAttributes);
        _children = new(getChildren);
    }
}
