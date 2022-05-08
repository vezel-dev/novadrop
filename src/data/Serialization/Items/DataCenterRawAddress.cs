namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawAddress : IDataCenterItem<DataCenterRawAddress>
{
    public ushort SegmentIndex;

    public ushort ElementIndex;

    public static void ReverseEndianness(ref DataCenterRawAddress item)
    {
        item.SegmentIndex = BinaryPrimitives.ReverseEndianness(item.SegmentIndex);
        item.ElementIndex = BinaryPrimitives.ReverseEndianness(item.ElementIndex);
    }
}
