namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawString : IDataCenterItem
{
    public uint Hash;

    public int Length;

    public int Index;

    public DataCenterRawAddress Address;

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawString);
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        Hash = reader.ReadUInt32();
        Length = reader.ReadInt32();
        Index = reader.ReadInt32();
        Address.Read(architecture, ref reader);
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteUInt32(Hash);
        writer.WriteInt32(Length);
        writer.WriteInt32(Index);
        Address.Write(architecture, ref writer);
    }
}
