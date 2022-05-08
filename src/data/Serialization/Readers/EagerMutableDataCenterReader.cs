using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Readers;

sealed class EagerMutableDataCenterReader : DataCenterReader
{
    public EagerMutableDataCenterReader(DataCenterLoadOptions options)
        : base(options)
    {
    }

    protected override EagerMutableDataCenterNode AllocateNode(
        DataCenterAddress address, DataCenterRawNode raw, object parent, string name, DataCenterKeys keys)
    {
        var node = new EagerMutableDataCenterNode(parent, name, keys, raw.AttributeCount, raw.ChildCount);

        ForEachAttribute(raw, node.Attributes, static (dict, name, value) =>
        {
            if (!dict.TryAdd(name, value))
                throw new InvalidDataException($"Attribute named '{name}' was already recorded earlier.");
        });

        ForEachChild(raw, node, node.Children, static (list, node) => list.Add(node));

        return node;
    }

    protected override EagerMutableDataCenterNode? ResolveNode(DataCenterAddress address, object parent)
    {
        return Unsafe.As<EagerMutableDataCenterNode>(CreateNode(address, parent));
    }
}
