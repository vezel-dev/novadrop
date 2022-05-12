namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class LazyMutableDataCenterNode : MutableDataCenterNode
{
    public override Dictionary<string, DataCenterValue> Attributes => _attributes!.Value;

    public override List<DataCenterNode> Children => _children!.Value;

    readonly Lazy<Dictionary<string, DataCenterValue>>? _attributes;

    readonly Lazy<List<DataCenterNode>>? _children;

    public LazyMutableDataCenterNode(
        object parent,
        string name,
        string? value,
        DataCenterKeys keys,
        Func<Dictionary<string, DataCenterValue>> getAttributes,
        Func<List<DataCenterNode>> getChildren)
        : base(parent, name, value, keys)
    {
        _attributes = new(getAttributes);
        _children = new(getChildren);
    }
}
