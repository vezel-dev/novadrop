namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public sealed class DataCenterKeys : IEquatable<DataCenterKeys>
{
    public static DataCenterKeys None { get; } = new();

    public string? AttributeName1 { get; }

    public string? AttributeName2 { get; }

    public string? AttributeName3 { get; }

    public string? AttributeName4 { get; }

    public IEnumerable<string> AttributeNames
    {
        get
        {
            if (AttributeName1 is string n1)
                yield return n1;

            if (AttributeName2 is string n2)
                yield return n2;

            if (AttributeName3 is string n3)
                yield return n3;

            if (AttributeName4 is string n4)
                yield return n4;
        }
    }

    public bool HasAttributeNames =>
        (AttributeName1, AttributeName2, AttributeName3, AttributeName4) is not (null, null, null, null);

    public DataCenterKeys(
        string? attributeName1 = null,
        string? attributeName2 = null,
        string? attributeName3 = null,
        string? attributeName4 = null)
    {
        static void CheckName(string? name, [CallerArgumentExpression("name")] string? paramName = null)
        {
            _ = name != DataCenterConstants.ValueAttributeName ? true : throw new ArgumentException(null, paramName);
        }

        CheckName(attributeName1);
        CheckName(attributeName2);
        CheckName(attributeName3);
        CheckName(attributeName4);

        AttributeName1 = attributeName1;
        AttributeName2 = attributeName2;
        AttributeName3 = attributeName3;
        AttributeName4 = attributeName4;
    }

    public static bool operator ==(DataCenterKeys? left, DataCenterKeys? right)
    {
        return EqualityComparer<DataCenterKeys>.Default.Equals(left, right);
    }

    public static bool operator !=(DataCenterKeys? left, DataCenterKeys? right)
    {
        return !(left == right);
    }

    public DataCenterKeys WithAttributeName1(string attributeName1)
    {
        return new(attributeName1, AttributeName2, AttributeName3, AttributeName4);
    }

    public DataCenterKeys WithAttributeName2(string attributeName2)
    {
        return new(AttributeName1, attributeName2, AttributeName3, AttributeName4);
    }

    public DataCenterKeys WithAttributeName3(string attributeName3)
    {
        return new(AttributeName1, AttributeName2, attributeName3, AttributeName4);
    }

    public DataCenterKeys WithAttributeName4(string attributeName4)
    {
        return new(AttributeName1, AttributeName2, AttributeName3, attributeName4);
    }

    public bool Equals(DataCenterKeys? other)
    {
        return AttributeName1 == other?.AttributeName1 &&
            AttributeName2 == other?.AttributeName2 &&
            AttributeName3 == other?.AttributeName3 &&
            AttributeName4 == other?.AttributeName4;
    }

    public override bool Equals(object? obj)
    {
        return obj is DataCenterKeys k && Equals(k);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AttributeName1, AttributeName2, AttributeName3, AttributeName4);
    }

    public override string ToString()
    {
        var sb = new StringBuilder("{");
        var attrs = AttributeNames.ToArray();

        for (var i = 0; i < attrs.Length; i++)
        {
            _ = sb.Append(attrs[i]);

            if (i != attrs.Length - 1)
                _ = sb.Append(", ");
        }

        return sb.Append('}').ToString();
    }
}
