using Vezel.Novadrop.Data.Serialization;

namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
abstract class MutableDataCenterNode : DataCenterNode
{
    public abstract override Dictionary<string, DataCenterValue> Attributes { get; }

    public abstract override List<DataCenterNode> Children { get; }

    public MutableDataCenterNode(object parent, string name, DataCenterValue value, DataCenterKeys keys)
        : base(parent, name, value, keys)
    {
    }

    public override sealed DataCenterNode CreateChild(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _ = name != DataCenterConstants.RootNodeName ? true : throw new ArgumentException(null, nameof(name));
        _ = Children.Count != DataCenterAddress.MaxValue.ElementIndex + 1
            ? true : throw new InvalidOperationException();

        var child = new UserDataCenterNode(this, name);

        Children.Add(child);

        return child;
    }

    public override sealed bool RemoveChild(DataCenterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _ = node.Parent == this ? true : throw new ArgumentException(null, nameof(node));

        return Children.Remove(node);
    }

    public override sealed void ClearChildren()
    {
        Children.Clear();
    }

    public override sealed void AddAttribute(string name, DataCenterValue value)
    {
        _ = name != DataCenterConstants.ValueAttributeName ? true : throw new ArgumentException(null, nameof(name));
        _ = !value.IsNull ? true : throw new ArgumentException(null, nameof(value));
        _ = Attributes.Count != DataCenterAddress.MaxValue.ElementIndex + 1 ?
            true : throw new InvalidOperationException();

        Attributes.Add(name, value);
    }

    public override sealed void SetAttribute(string name, DataCenterValue value)
    {
        _ = name != DataCenterConstants.ValueAttributeName ? true : throw new ArgumentException(null, nameof(name));
        _ = !value.IsNull ? true : throw new ArgumentException(null, nameof(value));

        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(Attributes, name, out var exists);

        if (!exists && Attributes.Count == DataCenterAddress.MaxValue.ElementIndex + 2)
        {
            _ = Attributes.Remove(name);

            throw new InvalidOperationException();
        }

        entry = value;
    }

    public override sealed bool RemoveAttribute(string name)
    {
        _ = name != DataCenterConstants.ValueAttributeName ? true : throw new ArgumentException(null, nameof(name));

        return Attributes.Remove(name);
    }

    public override sealed void ClearAttributes()
    {
        Attributes.Clear();
    }
}
