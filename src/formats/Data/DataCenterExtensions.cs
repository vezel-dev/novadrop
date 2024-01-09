namespace Vezel.Novadrop.Data;

public static class DataCenterExtensions
{
    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    public static DataCenterNode Child(this DataCenterNode node)
    {
        Check.Null(node);

        return node.Children.Single();
    }

    public static DataCenterNode Child(this DataCenterNode node, string name)
    {
        Check.Null(node);
        Check.Null(name);

        return node.Children.Single(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> Children(this DataCenterNode node, string name)
    {
        Check.Null(node);
        Check.Null(name);

        return node.Children.Where(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> Ancestors(this DataCenterNode node)
    {
        Check.Null(node);

        return node.Parent?.AncestorsAndSelf() ?? Array.Empty<DataCenterNode>();
    }

    public static IEnumerable<DataCenterNode> Ancestors(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.Ancestors().Where(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> AncestorsAndSelf(this DataCenterNode node)
    {
        Check.Null(node);

        var current = node;

        do
        {
            yield return current;

            current = current.Parent;
        }
        while (current != null);
    }

    public static IEnumerable<DataCenterNode> AncestorsAndSelf(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.AncestorsAndSelf().Where(n => n.Name == name);
    }

    public static DataCenterNode Sibling(this DataCenterNode node)
    {
        return node.Siblings().Single();
    }

    public static DataCenterNode Sibling(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.Siblings().Single(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> Siblings(this DataCenterNode node)
    {
        return node.SiblingsAndSelf().Where(n => n != node);
    }

    public static IEnumerable<DataCenterNode> Siblings(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.Siblings().Where(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> SiblingsAndSelf(this DataCenterNode node)
    {
        Check.Null(node);

        return node.Parent is { } parent ? parent.Children : [node];
    }

    public static IEnumerable<DataCenterNode> SiblingsAndSelf(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.SiblingsAndSelf().Where(n => n.Name == name);
    }

    public static DataCenterNode Descendant(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.Descendants().Single(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> Descendants(this DataCenterNode node)
    {
        return node.DescendantsAndSelf().Where(n => n != node);
    }

    public static IEnumerable<DataCenterNode> Descendants(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.Descendants().Where(n => n.Name == name);
    }

    public static IEnumerable<DataCenterNode> DescendantsAndSelf(this DataCenterNode node)
    {
        Check.Null(node);

        var work = new Stack<DataCenterNode>();

        work.Push(node);

        do
        {
            var current = work.Pop();

            yield return current;

            if (current.HasChildren)
                foreach (var child in current.Children.Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    public static IEnumerable<DataCenterNode> DescendantsAndSelf(this DataCenterNode node, string name)
    {
        Check.Null(name);

        return node.DescendantsAndSelf().Where(n => n.Name == name);
    }

    public static DataCenterNode DescendantAt(this DataCenterNode node, params string[] path)
    {
        return node.DescendantsAt(path).Single();
    }

    public static DataCenterNode DescendantAt(this DataCenterNode node, IEnumerable<string> path)
    {
        return node.DescendantsAt(path).Single();
    }

    public static IEnumerable<DataCenterNode> DescendantsAt(this DataCenterNode node, params string[] path)
    {
        Check.Null(node);
        Check.Null(path);
        Check.All(path, static name => name != null);

        var results = new List<DataCenterNode>();

        void Evaluate(DataCenterNode node, ReadOnlySpan<string> path)
        {
            if (!node.HasChildren)
                return;

            var name = path[0];
            var remaining = path[1..];

            foreach (var child in node.UnsafeChildren)
            {
                if (child.Name != name)
                    continue;

                if (remaining is [])
                    results.Add(child);
                else
                    Evaluate(child, remaining);
            }
        }

        Evaluate(node, path);

        return results;
    }

    public static IEnumerable<DataCenterNode> DescendantsAt(this DataCenterNode node, IEnumerable<string> path)
    {
        return node.DescendantsAt(path.ToArray());
    }

    public static int ToInt32(this DataCenterValue value, int radix = 10)
    {
        Check.Range(radix is 2 or 10 or 16, radix);

        return value.TypeCode switch
        {
            DataCenterTypeCode.Int32 => value.UnsafeAsInt32,
            DataCenterTypeCode.Single => (int)value.UnsafeAsSingle,
            DataCenterTypeCode.String => radix switch
            {
                2 => int.Parse(value.UnsafeAsString, NumberStyles.BinaryNumber, _culture),
                10 => int.Parse(value.UnsafeAsString, NumberStyles.Integer, _culture),
                16 => int.Parse(value.UnsafeAsString, NumberStyles.HexNumber, _culture),
                _ => throw new UnreachableException(),
            },
            DataCenterTypeCode.Boolean => value.UnsafeAsInt32, // Normalized internally; reinterpret as 0 or 1.
            _ => 0,
        };
    }

    public static float ToSingle(this DataCenterValue value)
    {
        return value.TypeCode switch
        {
            DataCenterTypeCode.Int32 => value.UnsafeAsInt32,
            DataCenterTypeCode.Single => value.UnsafeAsSingle,
            DataCenterTypeCode.String => float.Parse(value.UnsafeAsString, NumberStyles.Float, _culture),
            DataCenterTypeCode.Boolean => value.UnsafeAsInt32, // Normalized internally; reinterpret as 0 or 1.
            _ => 0.0f,
        };
    }

    public static bool ToBoolean(this DataCenterValue value)
    {
        return value.TypeCode switch
        {
            DataCenterTypeCode.Int32 => value.UnsafeAsInt32 switch
            {
                0 => false,
                _ => true,
            },
            DataCenterTypeCode.Single => value.UnsafeAsSingle != 0.0f,
            DataCenterTypeCode.String => value.UnsafeAsString switch
            {
                var s when bool.TryParse(s, out var b) => b,
                var s => s.AsSpan().Trim() switch
                {
                    "0" => false,
                    "1" => true,
                    _ => throw new FormatException(),
                },
            },
            DataCenterTypeCode.Boolean => value.UnsafeAsBoolean,
            _ => false,
        };
    }
}
