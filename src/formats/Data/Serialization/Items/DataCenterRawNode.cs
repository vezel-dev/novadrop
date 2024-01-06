namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawNode : IDataCenterItem, IEquatable<DataCenterRawNode>
{
    public ushort NameIndex;

    public ushort KeysInfo;

    public ushort AttributeCount;

    public ushort ChildCount;

    public DataCenterRawAddress AttributeAddress;

    public uint Padding1; // This can be safely ignored.

    public DataCenterRawAddress ChildAddress;

    public uint Padding2; // This can be safely ignored.

    public static bool operator ==(DataCenterRawNode left, DataCenterRawNode right) => left.Equals(right);

    public static bool operator !=(DataCenterRawNode left, DataCenterRawNode right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return architecture == DataCenterArchitecture.X64
            ? sizeof(DataCenterRawNode)
            : sizeof(DataCenterRawNode) - sizeof(uint) * 2;
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        NameIndex = reader.ReadUInt16();
        KeysInfo = reader.ReadUInt16();
        AttributeCount = reader.ReadUInt16();
        ChildCount = reader.ReadUInt16();
        AttributeAddress.Read(architecture, ref reader);

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));

        ChildAddress.Read(architecture, ref reader);

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteUInt16(NameIndex);
        writer.WriteUInt16(KeysInfo);
        writer.WriteUInt16(AttributeCount);
        writer.WriteUInt16(ChildCount);
        AttributeAddress.Write(architecture, ref writer);

        if (architecture == DataCenterArchitecture.X64)
            writer.Advance(sizeof(uint));

        ChildAddress.Write(architecture, ref writer);

        if (architecture == DataCenterArchitecture.X64)
            writer.Advance(sizeof(uint));
    }

    public readonly bool Equals(DataCenterRawNode other)
    {
        return (NameIndex, KeysInfo, AttributeCount, ChildCount, AttributeAddress, ChildAddress) ==
            (other.NameIndex,
             other.KeysInfo,
             other.AttributeCount,
             other.ChildCount,
             other.AttributeAddress,
             other.ChildAddress);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawNode n && Equals(n);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(NameIndex, KeysInfo, AttributeCount, ChildCount, AttributeAddress, ChildAddress);
    }
}
