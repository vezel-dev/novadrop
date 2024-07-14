namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawKeys : IDataCenterItem<DataCenterRawKeys>
{
    public ushort NameIndex1;

    public ushort NameIndex2;

    public ushort NameIndex3;

    public ushort NameIndex4;

    public static bool operator ==(DataCenterRawKeys left, DataCenterRawKeys right) => left.Equals(right);

    public static bool operator !=(DataCenterRawKeys left, DataCenterRawKeys right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawKeys);
    }

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawKeys item)
    {
        item.NameIndex1 = reader.ReadUInt16();
        item.NameIndex2 = reader.ReadUInt16();
        item.NameIndex3 = reader.ReadUInt16();
        item.NameIndex4 = reader.ReadUInt16();
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawKeys item)
    {
        writer.WriteUInt16(item.NameIndex1);
        writer.WriteUInt16(item.NameIndex2);
        writer.WriteUInt16(item.NameIndex3);
        writer.WriteUInt16(item.NameIndex4);
    }

    public readonly bool Equals(DataCenterRawKeys other)
    {
        return (NameIndex1, NameIndex2, NameIndex3, NameIndex4) ==
            (other.NameIndex1, other.NameIndex2, other.NameIndex3, other.NameIndex4);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawKeys k && Equals(k);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(NameIndex1, NameIndex2, NameIndex3, NameIndex4);
    }

    public override readonly string ToString()
    {
        return $"({NameIndex1}:{NameIndex2}:{NameIndex3}:{NameIndex4})";
    }
}
