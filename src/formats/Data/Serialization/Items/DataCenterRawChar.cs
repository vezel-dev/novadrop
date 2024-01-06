namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawChar : IDataCenterItem
{
    public char Value;

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawChar);
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        Value = reader.ReadChar();
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteChar(Value);
    }
}
