using Vezel.Novadrop.Data.Serialization;

namespace Vezel.Novadrop.Data.Nodes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal abstract class MutableDataCenterNode : DataCenterNode
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
        Check.Null(name);
        Check.Argument(name != DataCenterConstants.RootNodeName, name);
        Check.Operation(Children.Count != DataCenterAddress.MaxValue.ElementIndex + 1);

        var child = new UserDataCenterNode(this, name);

        Children.Add(child);

        return child;
    }

    public override sealed DataCenterNode CreateChildAt(int index, string name)
    {
        Check.Null(name);
        Check.Argument(name != DataCenterConstants.RootNodeName, name);
        Check.Operation(Children.Count != DataCenterAddress.MaxValue.ElementIndex + 1);

        var child = new UserDataCenterNode(this, name);

        Children.Insert(index, child);

        return child;
    }

    public override sealed bool RemoveChild(DataCenterNode node)
    {
        Check.Null(node);
        Check.Argument(node.Parent == this, node);

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
        Check.Null(comparer);

        Children.Sort(comparer);
    }

    public override sealed void AddAttribute(string name, DataCenterValue value)
    {
        Check.Argument(name != DataCenterConstants.ValueAttributeName, name);
        Check.Argument(!value.IsNull, value);
        Check.Operation(Attributes.Count != DataCenterAddress.MaxValue.ElementIndex + 1);

        Attributes.Add(name, value);
    }

    public override sealed void SetAttribute(string name, DataCenterValue value)
    {
        Check.Argument(name != DataCenterConstants.ValueAttributeName, name);
        Check.Argument(!value.IsNull, value);

        Attributes[name] = value;

        if (Attributes.Count == DataCenterAddress.MaxValue.ElementIndex + 2)
        {
            _ = Attributes.Remove(name);

            throw new InvalidOperationException();
        }
    }

    public override sealed bool RemoveAttribute(string name)
    {
        Check.Argument(name != DataCenterConstants.ValueAttributeName, name);

        return Attributes.Remove(name);
    }

    public override sealed void ClearAttributes()
    {
        Attributes.Clear();
    }
}
