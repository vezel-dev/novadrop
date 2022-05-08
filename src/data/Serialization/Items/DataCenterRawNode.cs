namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawNode : IDataCenterItem<DataCenterRawNode>
{
    public ushort NameIndex;

    public ushort KeysInfo;

    public ushort AttributeCount;

    public ushort ChildCount;

    public DataCenterRawAddress AttributeAddress;

    public uint Padding1;

    public DataCenterRawAddress ChildAddress;

    public uint Padding2;

    public static void ReverseEndianness(ref DataCenterRawNode item)
    {
        item.NameIndex = BinaryPrimitives.ReverseEndianness(item.NameIndex);
        item.KeysInfo = BinaryPrimitives.ReverseEndianness(item.KeysInfo);
        item.AttributeCount = BinaryPrimitives.ReverseEndianness(item.AttributeCount);
        item.ChildCount = BinaryPrimitives.ReverseEndianness(item.ChildCount);
        DataCenterRawAddress.ReverseEndianness(ref item.AttributeAddress);
        DataCenterRawAddress.ReverseEndianness(ref item.ChildAddress);

        // Note: Padding1 and Padding2 can be safely ignored.
    }
}
