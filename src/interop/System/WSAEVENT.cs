namespace Vezel.Novadrop.Interop.System;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct WSAEVENT :
    IEquatable<WSAEVENT>,
    IEqualityOperators<WSAEVENT, WSAEVENT, bool>,
    IComparable<WSAEVENT>,
    IComparisonOperators<WSAEVENT, WSAEVENT, bool>
{
    public static WSAEVENT WSA_INVALID_EVENT { get; } = new(value: null);

    private readonly void* _value;

    public WSAEVENT(void* value)
    {
        _value = value;
    }

    public static explicit operator void*(WSAEVENT value) => value._value;

    public static bool operator ==(WSAEVENT left, WSAEVENT right) => left.Equals(right);

    public static bool operator !=(WSAEVENT left, WSAEVENT right) => !left.Equals(right);

    public static bool operator <(WSAEVENT left, WSAEVENT right) => left.CompareTo(right) < 0;

    public static bool operator <=(WSAEVENT left, WSAEVENT right) => left.CompareTo(right) <= 0;

    public static bool operator >(WSAEVENT left, WSAEVENT right) => left.CompareTo(right) > 0;

    public static bool operator >=(WSAEVENT left, WSAEVENT right) => left.CompareTo(right) >= 0;

    public bool Equals(WSAEVENT other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is WSAEVENT w && Equals(w);
    }

    public int CompareTo(WSAEVENT other)
    {
        return ((nuint)_value).CompareTo((nuint)other._value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((nuint)_value);
    }

    public override string ToString()
    {
        return $"0x{(nuint)_value:x}";
    }
}
