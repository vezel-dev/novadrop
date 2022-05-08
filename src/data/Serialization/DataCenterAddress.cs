using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
readonly struct DataCenterAddress : IEquatable<DataCenterAddress>
{
    public static readonly DataCenterAddress MinValue = new(ushort.MinValue, ushort.MinValue);

    public static readonly DataCenterAddress MaxValue = new(ushort.MaxValue, ushort.MaxValue);

    public ushort SegmentIndex { get; }

    public ushort ElementIndex { get; }

    public DataCenterAddress(ushort segmentIndex, ushort elementIndex)
    {
        SegmentIndex = segmentIndex;
        ElementIndex = elementIndex;
    }

    public DataCenterAddress(DataCenterRawAddress raw)
        : this(raw.SegmentIndex, raw.ElementIndex)
    {
    }

    public static implicit operator DataCenterAddress(DataCenterRawAddress raw)
    {
        return new(raw);
    }

    public static implicit operator DataCenterRawAddress(DataCenterAddress address)
    {
        return new()
        {
            SegmentIndex = address.SegmentIndex,
            ElementIndex = address.ElementIndex,
        };
    }

    public static bool operator ==(DataCenterAddress left, DataCenterAddress right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataCenterAddress left, DataCenterAddress right)
    {
        return !left.Equals(right);
    }

    public bool Equals(DataCenterAddress other)
    {
        return SegmentIndex == other.SegmentIndex && ElementIndex == other.ElementIndex;
    }

    public override bool Equals(object? obj)
    {
        return obj is DataCenterAddress a && Equals(a);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SegmentIndex, ElementIndex);
    }

    public override string ToString()
    {
        return $"{{{SegmentIndex}:{ElementIndex}}}";
    }
}
