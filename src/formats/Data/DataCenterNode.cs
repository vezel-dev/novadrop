namespace Vezel.Novadrop.Data;

public abstract class DataCenterNode
{
    public DataCenterNode? Parent { get; }

    public string Name
    {
        get => _name;
        set => SetName(value);
    }

    public string? Value
    {
        get => _value;
        set => SetValue(value);
    }

    public DataCenterKeys Keys
    {
        get => _keys;
        set => SetKeys(value);
    }

    public abstract IReadOnlyDictionary<string, DataCenterValue> Attributes { get; }

    public virtual bool HasAttributes => Attributes.Count != 0;

    public abstract IReadOnlyList<DataCenterNode> Children { get; }

    public virtual bool HasChildren => Children.Count != 0;

    public abstract bool IsImmutable { get; }

    public DataCenterValue this[string name]
    {
        get => Attributes[name];
        set => SetAttribute(name, value);
    }

    private string _name;

    private string? _value;

    private DataCenterKeys _keys;

    private protected DataCenterNode(DataCenterNode? parent, string name, string? value, DataCenterKeys keys)
    {
        Parent = parent;
        _name = name;
        _value = value;
        _keys = keys;
    }

    private protected virtual void SetName(string name)
    {
        Check.Null(name);
        Check.Argument(name != DataCenterConstants.RootNodeName, name);

        _name = name;
    }

    private protected virtual void SetValue(string? value)
    {
        _value = value;
    }

    private protected virtual void SetKeys(DataCenterKeys keys)
    {
        Check.Null(keys);

        _keys = keys;
    }

    public abstract DataCenterNode CreateChild(string name);

    public abstract DataCenterNode CreateChildAt(int index, string name);

    public abstract bool RemoveChild(DataCenterNode node);

    public abstract void RemoveChildAt(int index);

    public abstract void RemoveChildRange(int index, int count);

    public abstract void ClearChildren();

    public void ReverseChildren()
    {
        ReverseChildren(0, Children.Count);
    }

    public abstract void ReverseChildren(int index, int count);

    public abstract void SortChildren(IComparer<DataCenterNode> comparer);

    public abstract void AddAttribute(string name, DataCenterValue value);

    private protected abstract void SetAttribute(string name, DataCenterValue value);

    public abstract bool RemoveAttribute(string name);

    public abstract void ClearAttributes();

    public abstract void TrimExcess();

    public override string ToString()
    {
        return $"{{Name: {_name}, Value: {_value}, Keys: {_keys}, " +
            $"Attributes: [{Attributes.Count}], Children: [{Children.Count}]}}";
    }
}
