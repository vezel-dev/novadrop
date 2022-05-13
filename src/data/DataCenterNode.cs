namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public abstract class DataCenterNode
{
    public DataCenter Owner => _parent is DataCenter dc ? dc : Unsafe.As<DataCenterNode>(_parent).Owner;

    public DataCenterNode? Parent => _parent is not DataCenter ? Unsafe.As<DataCenterNode>(_parent) : null;

    public string Name { get; }

    public virtual string? Value
    {
        get => _value;
        set => _value = value;
    }

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

    public virtual bool HasAttributes => Attributes.Count != 0;

    public abstract IReadOnlyCollection<DataCenterNode> Children { get; }

    public virtual bool HasChildren => Children.Count != 0;

    public abstract bool IsImmutable { get; }

    public DataCenterValue this[string name]
    {
        get => Attributes[name];
        set => SetAttribute(name, value);
    }

    readonly object _parent;

    string? _value;

    DataCenterKeys _keys;

    private protected DataCenterNode(object parent, string name, string? value, DataCenterKeys keys)
    {
        _parent = parent;
        Name = name;
        _value = value;
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

            if (!current.HasChildren)
                continue;

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
        return $"{{Name: {Name}, Value: {Value}, Keys: {Keys}, " +
            $"Attributes: [{Attributes.Count}], Children: [{Children.Count}]}}";
    }
}
