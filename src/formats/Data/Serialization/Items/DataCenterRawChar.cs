namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawChar : IDataCenterItem
{
    public char Value;

    public void ReverseEndianness()
    {
        Value = (char)BinaryPrimitives.ReverseEndianness(Value);
    }
}
