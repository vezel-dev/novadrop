namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawNode : IDataCenterItem, IEquatable<DataCenterRawNode>
{
    public ushort NameIndex;

    public ushort KeysInfo;

    public ushort AttributeCount;

    public ushort ChildCount;

    public DataCenterRawAddress AttributeAddress;

    public uint Padding1;

    public DataCenterRawAddress ChildAddress;

    public uint Padding2;

    public void ReverseEndianness()
    {
        NameIndex = BinaryPrimitives.ReverseEndianness(NameIndex);
        KeysInfo = BinaryPrimitives.ReverseEndianness(KeysInfo);
        AttributeCount = BinaryPrimitives.ReverseEndianness(AttributeCount);
        ChildCount = BinaryPrimitives.ReverseEndianness(ChildCount);
        AttributeAddress.ReverseEndianness();
        ChildAddress.ReverseEndianness();

        // Note: Padding1 and Padding2 can be safely ignored.
    }

    public static bool operator ==(DataCenterRawNode left, DataCenterRawNode right) => left.Equals(right);

    public static bool operator !=(DataCenterRawNode left, DataCenterRawNode right) => !left.Equals(right);

    public bool Equals(DataCenterRawNode other)
    {
        return (NameIndex, KeysInfo, AttributeCount, ChildCount, AttributeAddress, ChildAddress) ==
            (other.NameIndex,
             other.KeysInfo,
             other.AttributeCount,
             other.ChildCount,
             other.AttributeAddress,
             other.ChildAddress);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawNode n && Equals(n);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NameIndex, KeysInfo, AttributeCount, ChildCount, AttributeAddress, ChildAddress);
    }
}
