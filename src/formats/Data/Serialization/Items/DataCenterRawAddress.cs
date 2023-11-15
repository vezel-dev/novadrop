namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAddress : IDataCenterItem, IEquatable<DataCenterRawAddress>
{
    public ushort SegmentIndex;

    public ushort ElementIndex;

    public void ReverseEndianness()
    {
        SegmentIndex = BinaryPrimitives.ReverseEndianness(SegmentIndex);
        ElementIndex = BinaryPrimitives.ReverseEndianness(ElementIndex);
    }

    public static bool operator ==(DataCenterRawAddress left, DataCenterRawAddress right) => left.Equals(right);

    public static bool operator !=(DataCenterRawAddress left, DataCenterRawAddress right) => !left.Equals(right);

    public readonly bool Equals(DataCenterRawAddress other)
    {
        return (SegmentIndex, ElementIndex) == (other.SegmentIndex, other.ElementIndex);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawAddress a && Equals(a);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(SegmentIndex, ElementIndex);
    }
}
