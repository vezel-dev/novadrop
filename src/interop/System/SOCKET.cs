// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Interop.System;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct SOCKET :
    IEquatable<SOCKET>,
    IEqualityOperators<SOCKET, SOCKET, bool>,
    IComparable<SOCKET>,
    IComparisonOperators<SOCKET, SOCKET, bool>
{
    public static SOCKET INVALID_SOCKET { get; } = new(value: ~0u);

    private readonly ulong _value;

    public SOCKET(ulong value)
    {
        _value = value;
    }

    public static explicit operator ulong(SOCKET value) => value._value;

    public static bool operator ==(SOCKET left, SOCKET right) => left.Equals(right);

    public static bool operator !=(SOCKET left, SOCKET right) => !left.Equals(right);

    public static bool operator <(SOCKET left, SOCKET right) => left.CompareTo(right) < 0;

    public static bool operator <=(SOCKET left, SOCKET right) => left.CompareTo(right) <= 0;

    public static bool operator >(SOCKET left, SOCKET right) => left.CompareTo(right) > 0;

    public static bool operator >=(SOCKET left, SOCKET right) => left.CompareTo(right) >= 0;

    public bool Equals(SOCKET other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SOCKET s && Equals(s);
    }

    public int CompareTo(SOCKET other)
    {
        return _value.CompareTo(other._value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value);
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }
}
