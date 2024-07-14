namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAttribute : IDataCenterItem<DataCenterRawAttribute>
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

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawAttribute item)
    {
        item.NameIndex = reader.ReadUInt16();
        item.TypeInfo = reader.ReadUInt16();
        item.Value = reader.ReadInt32();

        Unsafe.SkipInit(out item.Padding1);

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawAttribute item)
    {
        writer.WriteUInt16(item.NameIndex);
        writer.WriteUInt16(item.TypeInfo);
        writer.WriteInt32(item.Value);

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

    public override readonly string ToString()
    {
        return $"({NameIndex}:{TypeInfo}:{Value})";
    }
}
