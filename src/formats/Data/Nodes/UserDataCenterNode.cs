namespace Vezel.Novadrop.Data.Nodes;

internal sealed class UserDataCenterNode : MutableDataCenterNode
{
    public override OrderedDictionary<string, DataCenterValue> Attributes => _attributes ??= [];

    public override List<DataCenterNode> Children => _children ??= [];

    private OrderedDictionary<string, DataCenterValue>? _attributes;

    private List<DataCenterNode>? _children;

    public UserDataCenterNode(DataCenterNode? parent, string name)
        : base(parent, name, default, DataCenterKeys.None)
    {
    }
}
