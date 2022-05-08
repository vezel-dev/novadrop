namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
sealed class UserDataCenterNode : MutableDataCenterNode
{
    public override Dictionary<string, DataCenterValue> Attributes { get; } = new();

    public override List<DataCenterNode> Children { get; } = new();

    public UserDataCenterNode(object parent, string name)
        : base(parent, name, DataCenterKeys.None)
    {
    }
}
