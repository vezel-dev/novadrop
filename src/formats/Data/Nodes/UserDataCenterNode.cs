// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Data.Nodes;

internal sealed class UserDataCenterNode : MutableDataCenterNode
{
    public override OrderedDictionary<string, DataCenterValue> Attributes => _attributes ??= [];

    public override bool HasAttributes => _attributes is [_, ..];

    public override List<DataCenterNode> Children => _children ??= [];

    public override bool HasChildren => _children is [_, ..];

    private OrderedDictionary<string, DataCenterValue>? _attributes;

    private List<DataCenterNode>? _children;

    public UserDataCenterNode(DataCenterNode? parent, string name)
        : base(parent, name, value: null, DataCenterKeys.None)
    {
    }
}
