namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAttribute : IDataCenterItem, IEquatable<DataCenterRawAttribute>
{
    public ushort NameIndex;

    public ushort TypeInfo;

    public int Value;

    public uint Padding1;

    public void ReverseEndianness()
    {
        NameIndex = BinaryPrimitives.ReverseEndianness(NameIndex);
        TypeInfo = BinaryPrimitives.ReverseEndianness(TypeInfo);
        Value = BinaryPrimitives.ReverseEndianness(Value);

        // Note: Padding1 can be safely ignored.
    }

    public static bool operator ==(DataCenterRawAttribute left, DataCenterRawAttribute right) => left.Equals(right);

    public static bool operator !=(DataCenterRawAttribute left, DataCenterRawAttribute right) => !left.Equals(right);

    public bool Equals(DataCenterRawAttribute other)
    {
        return (NameIndex, TypeInfo, Value) == (other.NameIndex, other.TypeInfo, other.Value);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawAttribute a && Equals(a);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NameIndex, TypeInfo, Value);
    }
}
