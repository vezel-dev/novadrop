namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawString : IDataCenterItem<DataCenterRawString>
{
    public uint Hash;

    public int Length;

    public int Index;

    public DataCenterRawAddress Address;

    public static void ReverseEndianness(ref DataCenterRawString item)
    {
        item.Hash = BinaryPrimitives.ReverseEndianness(item.Hash);
        item.Length = BinaryPrimitives.ReverseEndianness(item.Length);
        item.Index = BinaryPrimitives.ReverseEndianness(item.Index);
        DataCenterRawAddress.ReverseEndianness(ref item.Address);
    }
}
