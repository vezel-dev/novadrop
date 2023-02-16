namespace Vezel.Novadrop.Schema;

internal sealed class DataCenterNodeSchema
{
    public IEnumerable<string> Keys => _keys ?? Array.Empty<string>();

    public IReadOnlyDictionary<string, DataCenterAttributeSchema> Attributes => _attributes;

    public IReadOnlyDictionary<string, DataCenterEdgeSchema> Children => _children;

    public bool HasValue { get; set; }

    public bool HasKeys => _keys != null;

    public bool HasMixedContent => HasValue && _children.Count != 0;

    private readonly Dictionary<string, DataCenterAttributeSchema> _attributes = new();

    private readonly Dictionary<string, DataCenterEdgeSchema> _children = new();

    private IEnumerable<string>? _keys;

    public void SetKeys(IEnumerable<string> keys)
    {
        _keys = keys;
    }

    public void UpdateAttributeBounds(
        bool nodeExistedBefore, IReadOnlyDictionary<string, DataCenterTypeCode> currentAttributeTypes)
    {
        foreach (var name in currentAttributeTypes.Keys.Concat(_attributes.Keys))
        {
            if (currentAttributeTypes.TryGetValue(name, out var code))
            {
                ref var attr = ref CollectionsMarshal.GetValueRefOrAddDefault(_attributes, name, out _);

                attr ??= new(code)
                {
                    IsOptional = nodeExistedBefore,
                };
            }
            else
                Attributes[name].IsOptional = true;
        }
    }

    public void UpdateEdgeBounds(IReadOnlyDictionary<string, int> childCounts)
    {
        foreach (var name in _children.Keys.Concat(childCounts.Keys))
        {
            var count = childCounts.GetValueOrDefault(name);
            var child = _children[name];

            if (count == 0)
                child.IsOptional = true;
            else if (count > 1)
                child.IsRepeatable = true;
        }
    }

    public DataCenterEdgeSchema GetEdge(
        DataCenterSchemaStrategy strategy,
        string parentName,
        string childName,
        out bool edgeExistedBefore,
        out bool childSchemaExistedBefore)
    {
        ref var edge = ref CollectionsMarshal.GetValueRefOrAddDefault(_children, childName, out edgeExistedBefore);

        childSchemaExistedBefore = edgeExistedBefore;

        if (strategy == DataCenterSchemaStrategy.Aggressive && childName == parentName)
        {
            edge ??= new(this);

            childSchemaExistedBefore = true;
        }
        else
            edge ??= new(new());

        return edge;
    }
}
