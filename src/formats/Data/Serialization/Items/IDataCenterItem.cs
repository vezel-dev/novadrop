namespace Vezel.Novadrop.Data.Serialization.Items;

internal interface IDataCenterItem
{
    public static abstract int GetSize(DataCenterArchitecture architecture);

    public abstract void Read(DataCenterArchitecture architecture, ref SpanReader reader);

    public abstract void Write(DataCenterArchitecture architecture, ref SpanWriter writer);
}
