namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DataCenterRawChar : IDataCenterItem<DataCenterRawChar>
{
    public char Value;

    public static void ReverseEndianness(ref DataCenterRawChar item)
    {
        item.Value = (char)BinaryPrimitives.ReverseEndianness(item.Value);
    }
}
