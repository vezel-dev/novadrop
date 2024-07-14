// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawNode : IDataCenterItem<DataCenterRawNode>
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

    public static void Read(ref SpanReader reader, DataCenterArchitecture architecture, out DataCenterRawNode item)
    {
        item.NameIndex = reader.ReadUInt16();
        item.KeysInfo = reader.ReadUInt16();
        item.AttributeCount = reader.ReadUInt16();
        item.ChildCount = reader.ReadUInt16();

        DataCenterRawAddress.Read(ref reader, architecture, out item.AttributeAddress);
        Unsafe.SkipInit(out item.Padding1);

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));

        DataCenterRawAddress.Read(ref reader, architecture, out item.ChildAddress);
        Unsafe.SkipInit(out item.Padding2);

        if (architecture == DataCenterArchitecture.X64)
            reader.Advance(sizeof(uint));
    }

    public static void Write(ref SpanWriter writer, DataCenterArchitecture architecture, in DataCenterRawNode item)
    {
        writer.WriteUInt16(item.NameIndex);
        writer.WriteUInt16(item.KeysInfo);
        writer.WriteUInt16(item.AttributeCount);
        writer.WriteUInt16(item.ChildCount);

        DataCenterRawAddress.Write(ref writer, architecture, item.AttributeAddress);

        if (architecture == DataCenterArchitecture.X64)
            writer.Advance(sizeof(uint));

        DataCenterRawAddress.Write(ref writer, architecture, item.ChildAddress);

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

    public override readonly string ToString()
    {
        return $"({NameIndex}:{KeysInfo}:{AttributeCount}:{ChildCount}:{AttributeAddress}:{ChildAddress})";
    }
}
