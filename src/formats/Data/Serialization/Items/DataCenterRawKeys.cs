namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawKeys : IDataCenterItem
{
    public ushort NameIndex1;

    public ushort NameIndex2;

    public ushort NameIndex3;

    public ushort NameIndex4;

    public void ReverseEndianness()
    {
        NameIndex1 = BinaryPrimitives.ReverseEndianness(NameIndex1);
        NameIndex2 = BinaryPrimitives.ReverseEndianness(NameIndex2);
        NameIndex3 = BinaryPrimitives.ReverseEndianness(NameIndex3);
        NameIndex4 = BinaryPrimitives.ReverseEndianness(NameIndex4);
    }
}
