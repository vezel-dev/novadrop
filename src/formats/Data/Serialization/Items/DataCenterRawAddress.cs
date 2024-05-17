// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Data.Serialization.Items;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DataCenterRawAddress : IDataCenterItem, IEquatable<DataCenterRawAddress>
{
    public ushort SegmentIndex;

    public ushort ElementIndex;

    public static bool operator ==(DataCenterRawAddress left, DataCenterRawAddress right) => left.Equals(right);

    public static bool operator !=(DataCenterRawAddress left, DataCenterRawAddress right) => !left.Equals(right);

    public static unsafe int GetSize(DataCenterArchitecture architecture)
    {
        return sizeof(DataCenterRawAddress);
    }

    public void Read(DataCenterArchitecture architecture, ref SpanReader reader)
    {
        SegmentIndex = reader.ReadUInt16();
        ElementIndex = reader.ReadUInt16();
    }

    public readonly void Write(DataCenterArchitecture architecture, ref SpanWriter writer)
    {
        writer.WriteUInt16(SegmentIndex);
        writer.WriteUInt16(ElementIndex);
    }

    public readonly bool Equals(DataCenterRawAddress other)
    {
        return (SegmentIndex, ElementIndex) == (other.SegmentIndex, other.ElementIndex);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterRawAddress a && Equals(a);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(SegmentIndex, ElementIndex);
    }
}
