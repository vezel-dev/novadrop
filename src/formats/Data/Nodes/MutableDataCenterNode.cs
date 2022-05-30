using Vezel.Novadrop.Data.Serialization;

namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
abstract class MutableDataCenterNode : DataCenterNode
{
    public abstract override OrderedDictionary<string, DataCenterValue> Attributes { get; }

    public abstract override List<DataCenterNode> Children { get; }

    public override sealed bool IsImmutable => false;

    public MutableDataCenterNode(DataCenterNode? parent, string name, string? value, DataCenterKeys keys)
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

    public override sealed DataCenterNode CreateChildAt(int index, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _ = name != DataCenterConstants.RootNodeName ? true : throw new ArgumentException(null, nameof(name));
        _ = Children.Count != DataCenterAddress.MaxValue.ElementIndex + 1
            ? true : throw new InvalidOperationException();

        var child = new UserDataCenterNode(this, name);

        Children.Insert(index, child);

        return child;
    }

    public override sealed bool RemoveChild(DataCenterNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _ = node.Parent == this ? true : throw new ArgumentException(null, nameof(node));

        return Children.Remove(node);
    }

    public override sealed void RemoveChildAt(int index)
    {
        Children.RemoveAt(index);
    }

    public override sealed void RemoveChildRange(int index, int count)
    {
        Children.RemoveRange(index, count);
    }

    public override sealed void ClearChildren()
    {
        Children.Clear();
    }

    public override void ReverseChildren(int index, int count)
    {
        Children.Reverse(index, count);
    }

    public override sealed void SortChildren(IComparer<DataCenterNode> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        Children.Sort(comparer);
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

        Attributes[name] = value;

        if (Attributes.Count == DataCenterAddress.MaxValue.ElementIndex + 2)
        {
            _ = Attributes.Remove(name);

            throw new InvalidOperationException();
        }
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
