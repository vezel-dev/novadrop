namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal sealed class UserDataCenterNode : MutableDataCenterNode
{
    public override OrderedDictionary<string, DataCenterValue> Attributes => _attributes ??= new();

    public override List<DataCenterNode> Children => _children ??= new();

    private OrderedDictionary<string, DataCenterValue>? _attributes;

    private List<DataCenterNode>? _children;

    public UserDataCenterNode(DataCenterNode? parent, string name)
        : base(parent, name, default, DataCenterKeys.None)
    {
    }
}
