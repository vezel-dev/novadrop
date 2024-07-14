namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAddress : IDataCenterItem<DataCenterRawAddress>
{
    public ushort SegmentIndex;

    public ushort ElementIndex;

    public static bool operator ==(DataCenterRawAddress left, DataCenterRawAddress right) => left.Equals(right);

    public static bool operator !=(DataCenterRawAddress left, DataCenterRawAddress right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawAddress);
    }

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawAddress item)
    {
        item.SegmentIndex = reader.ReadUInt16();
        item.ElementIndex = reader.ReadUInt16();
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawAddress item)
    {
        writer.WriteUInt16(item.SegmentIndex);
        writer.WriteUInt16(item.ElementIndex);
    }

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

    public override readonly string ToString()
    {
        return $"({SegmentIndex}:{ElementIndex})";
    }
}
