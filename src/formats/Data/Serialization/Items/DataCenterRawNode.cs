namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawNode : IDataCenterItem
{
    public ushort NameIndex;

    public ushort KeysInfo;

    public ushort AttributeCount;

    public ushort ChildCount;

    public DataCenterRawAddress AttributeAddress;

    public uint Padding1;

    public DataCenterRawAddress ChildAddress;

    public uint Padding2;

    public void ReverseEndianness()
    {
        NameIndex = BinaryPrimitives.ReverseEndianness(NameIndex);
        KeysInfo = BinaryPrimitives.ReverseEndianness(KeysInfo);
        AttributeCount = BinaryPrimitives.ReverseEndianness(AttributeCount);
        ChildCount = BinaryPrimitives.ReverseEndianness(ChildCount);
        AttributeAddress.ReverseEndianness();
        ChildAddress.ReverseEndianness();

        // Note: Padding1 and Padding2 can be safely ignored.
    }
}
