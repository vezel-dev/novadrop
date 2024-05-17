// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Interop.System;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct in_addr :
    IEquatable<in_addr>,
    IEqualityOperators<in_addr, in_addr, bool>,
    IComparable<in_addr>,
    IComparisonOperators<in_addr, in_addr, bool>
{
    private readonly uint _value;

    public in_addr(uint value)
    {
        _value = value;
    }

    public static explicit operator uint(in_addr value) => value._value;

    public static bool operator ==(in_addr left, in_addr right) => left.Equals(right);

    public static bool operator !=(in_addr left, in_addr right) => !left.Equals(right);

    public static bool operator <(in_addr left, in_addr right) => left.CompareTo(right) < 0;

    public static bool operator <=(in_addr left, in_addr right) => left.CompareTo(right) <= 0;

    public static bool operator >(in_addr left, in_addr right) => left.CompareTo(right) > 0;

    public static bool operator >=(in_addr left, in_addr right) => left.CompareTo(right) >= 0;

    public bool Equals(in_addr other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is in_addr a && Equals(a);
    }

    public int CompareTo(in_addr other)
    {
        return _value.CompareTo(other._value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value);
    }

    public override string ToString()
    {
        return $"{(byte)_value}.{(byte)(_value >> 8)}.{(byte)(_value >> 16)}.{(byte)(_value >> 24)}";
    }
}
