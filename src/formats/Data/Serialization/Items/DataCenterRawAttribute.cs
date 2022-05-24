namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawAttribute : IDataCenterItem
{
    public ushort NameIndex;

    public ushort TypeInfo;

    public int Value;

    public uint Padding1;

    public void ReverseEndianness()
    {
        NameIndex = BinaryPrimitives.ReverseEndianness(NameIndex);
        TypeInfo = BinaryPrimitives.ReverseEndianness(TypeInfo);
        Value = BinaryPrimitives.ReverseEndianness(Value);

        // Note: Padding1 can be safely ignored.
    }
}
