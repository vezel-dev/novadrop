namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawKeys : IDataCenterItem<DataCenterRawKeys>
{
    public ushort NameIndex1;

    public ushort NameIndex2;

    public ushort NameIndex3;

    public ushort NameIndex4;

    public static void ReverseEndianness(ref DataCenterRawKeys item)
    {
        item.NameIndex1 = BinaryPrimitives.ReverseEndianness(item.NameIndex1);
        item.NameIndex2 = BinaryPrimitives.ReverseEndianness(item.NameIndex2);
        item.NameIndex3 = BinaryPrimitives.ReverseEndianness(item.NameIndex3);
        item.NameIndex4 = BinaryPrimitives.ReverseEndianness(item.NameIndex4);
    }
}
