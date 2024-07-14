namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawString : IDataCenterItem<DataCenterRawString>
{
    public uint Hash;

    public int Length;

    public int Index;

    public DataCenterRawAddress Address;

    public static bool operator ==(DataCenterRawString left, DataCenterRawString right) => left.Equals(right);

    public static bool operator !=(DataCenterRawString left, DataCenterRawString right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawString);
    }

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawString item)
    {
        item.Hash = reader.ReadUInt32();
        item.Length = reader.ReadInt32();
        item.Index = reader.ReadInt32();

        DataCenterRawAddress.Read(ref reader, architecture, out item.Address);
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawString item)
    {
        writer.WriteUInt32(item.Hash);
        writer.WriteInt32(item.Length);
        writer.WriteInt32(item.Index);

        DataCenterRawAddress.Write(ref writer, architecture, item.Address);
    }

    public readonly bool Equals(DataCenterRawString other)
    {
        return (Hash, Length, Index, Address) == (other.Hash, other.Length, other.Index, other.Address);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawString s && Equals(s);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Hash, Length, Index, Address);
    }

    public override readonly string ToString()
    {
        return $"({Hash}:{Length}:{Index}:{Address})";
    }
}
