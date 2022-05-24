namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawAddress : IDataCenterItem
{
    public ushort SegmentIndex;

    public ushort ElementIndex;

    public void ReverseEndianness()
    {
        SegmentIndex = BinaryPrimitives.ReverseEndianness(SegmentIndex);
        ElementIndex = BinaryPrimitives.ReverseEndianness(ElementIndex);
    }
}
