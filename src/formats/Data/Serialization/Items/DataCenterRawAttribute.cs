namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawAttribute : IDataCenterItem<DataCenterRawAttribute>
{
    public ushort NameIndex;

    public ushort TypeInfo;

    public int Value;

    public uint Padding1;

    public static void ReverseEndianness(ref DataCenterRawAttribute item)
    {
        item.NameIndex = BinaryPrimitives.ReverseEndianness(item.NameIndex);
        item.TypeInfo = BinaryPrimitives.ReverseEndianness(item.TypeInfo);
        item.Value = BinaryPrimitives.ReverseEndianness(item.Value);

        // Note: Padding1 can be safely ignored.
    }
}
