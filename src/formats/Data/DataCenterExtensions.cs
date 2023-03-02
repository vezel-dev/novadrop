namespace Vezel.Novadrop.Data;

public static class DataCenterExtensions
{
    public static IEnumerable<DataCenterNode> Ancestors(this DataCenterNode node)
    {
        Check.Null(node);

        var current = node;

        while ((current = current.Parent) != null)
            yield return current;
    }

    public static IEnumerable<DataCenterNode> Siblings(this DataCenterNode node)
    {
        Check.Null(node);

        var parent = node.Parent;

        if (parent == null)
            yield break;

        foreach (var elem in parent.Children.Where(x => x != node))
            yield return elem;
    }

    public static IEnumerable<DataCenterNode> Descendants(this DataCenterNode node)
    {
        Check.Null(node);

        var work = new Stack<DataCenterNode>();

        work.Push(node);

        do
        {
            var current = work.Pop();

            if (current != node)
                yield return current;

            if (current.HasChildren)
                foreach (var child in current.Children.Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    public static int ToInt32(this DataCenterValue value)
    {
        return value.TypeCode switch
        {
            DataCenterTypeCode.Int32 => value.UnsafeAsInt32,
            DataCenterTypeCode.Single => (int)value.UnsafeAsSingle,
            DataCenterTypeCode.String => value.UnsafeAsString switch
            {
                var s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
                var s => int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
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
            DataCenterTypeCode.String =>
                float.Parse(value.UnsafeAsString, NumberStyles.Float, CultureInfo.InvariantCulture),
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
