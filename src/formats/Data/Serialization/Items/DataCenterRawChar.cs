namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawChar : IDataCenterItem<DataCenterRawChar>
{
    public char Value;

    public static bool operator ==(DataCenterRawChar left, DataCenterRawChar right) => left.Equals(right);

    public static bool operator !=(DataCenterRawChar left, DataCenterRawChar right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawChar);
    }

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawChar item)
    {
        item.Value = reader.ReadChar();
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawChar item)
    {
        writer.WriteChar(item.Value);
    }

    public readonly bool Equals(DataCenterRawChar other)
    {
        return Value == other.Value;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawChar c && Equals(c);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override readonly string ToString()
    {
        return $"({Value})";
    }
}
