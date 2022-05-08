namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public sealed class DataCenterKeys
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
}
