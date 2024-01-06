namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAttribute : IDataCenterItem, IEquatable<DataCenterRawAttribute>
{
    public ushort NameIndex;

    public ushort TypeInfo;

    public int Value;

    public uint Padding1; // This can be safely ignored.

    public static bool operator ==(DataCenterRawAttribute left, DataCenterRawAttribute right) => left.Equals(right);

    public static bool operator !=(DataCenterRawAttribute left, DataCenterRawAttribute right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return architecture == DataCenterArchitecture.X64
            ? sizeof(DataCenterRawAttribute)
            : sizeof(DataCenterRawAttribute) - sizeof(uint);
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        NameIndex = reader.ReadUInt16();
        TypeInfo = reader.ReadUInt16();
        Value = reader.ReadInt32();

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteUInt16(NameIndex);
        writer.WriteUInt16(TypeInfo);
        writer.WriteInt32(Value);

        if (architecture == DataCenterArchitecture.X64)
            writer.Advance(sizeof(uint));
    }

    public readonly bool Equals(DataCenterRawAttribute other)
    {
        return (NameIndex, TypeInfo, Value) == (other.NameIndex, other.TypeInfo, other.Value);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawAttribute a && Equals(a);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(NameIndex, TypeInfo, Value);
    }
}
