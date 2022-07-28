namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawString : IDataCenterItem
{
    public uint Hash;

    public int Length;

    public int Index;

    public DataCenterRawAddress Address;

    public void ReverseEndianness()
    {
        Hash = BinaryPrimitives.ReverseEndianness(Hash);
        Length = BinaryPrimitives.ReverseEndianness(Length);
        Index = BinaryPrimitives.ReverseEndianness(Index);
        Address.ReverseEndianness();
    }
}
