namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public abstract class DataCenterNode
{
    public DataCenter Owner => _parent is DataCenter dc ? dc : Unsafe.As<DataCenterNode>(_parent).Owner;

    public DataCenterNode? Parent => _parent is not DataCenter ? Unsafe.As<DataCenterNode>(_parent) : null;

    public string Name { get; }

    public virtual DataCenterKeys Keys
    {
        get => _keys;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _keys = value;
        }
    }

    public abstract IReadOnlyDictionary<string, DataCenterValue> Attributes { get; }

    public abstract IReadOnlyCollection<DataCenterNode> Children { get; }

    // TODO: Treat this specially; filter it from Attributes?
    public DataCenterValue Value
    {
        get => this[DataCenterConstants.ValueAttributeName];
        set => this[DataCenterConstants.ValueAttributeName] = value;
    }

    public DataCenterValue this[string name]
    {
        get => Attributes[name];
        set => SetAttribute(name, value);
    }

    readonly object _parent;

    DataCenterKeys _keys;

    private protected DataCenterNode(object parent, string name, DataCenterKeys keys)
    {
        _parent = parent;
        Name = name;
        _keys = keys;
    }

    public IEnumerable<DataCenterNode> Ancestors()
    {
        var current = this;

        while ((current = current.Parent) != null)
            yield return current;
    }

    public IEnumerable<DataCenterNode> Siblings()
    {
        var parent = Parent;

        if (parent == null)
            yield break;

        foreach (var elem in parent.Children.Where(x => x != this))
            yield return elem;
    }

    public IEnumerable<DataCenterNode> Descendants()
    {
        var work = new Queue<DataCenterNode>();

        work.Enqueue(this);

        while (work.Count != 0)
        {
            var current = work.Dequeue();

            foreach (var elem in current.Children)
            {
                yield return elem;

                work.Enqueue(elem);
            }
        }
    }

    public abstract DataCenterNode CreateChild(string name);

    public abstract bool RemoveChild(DataCenterNode node);

    public abstract void ClearChildren();

    public abstract void AddAttribute(string name, DataCenterValue value);

    public abstract void SetAttribute(string name, DataCenterValue value);

    public abstract bool RemoveAttribute(string name);

    public abstract void ClearAttributes();

    public override string ToString()
    {
        return $"{{Name: {Name}, Value: {Value}, Attributes: [{Attributes.Count}], Children: [{Children.Count}]}}";
    }
}
