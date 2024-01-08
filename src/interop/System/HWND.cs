namespace Vezel.Novadrop.Interop.System;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct HWND :
    IEquatable<HWND>,
    IEqualityOperators<HWND, HWND, bool>,
    IComparable<HWND>,
    IComparisonOperators<HWND, HWND, bool>
{
    public static HWND INVALID_HANDLE_VALUE { get; } = new(value: (void*)-1);

    public static HWND NULL { get; } = new(value: null);

    private readonly void* _value;

    public HWND(void* value)
    {
        _value = value;
    }

    public static explicit operator void*(HWND value) => value._value;

    public static bool operator ==(HWND left, HWND right) => left.Equals(right);

    public static bool operator !=(HWND left, HWND right) => !left.Equals(right);

    public static bool operator <(HWND left, HWND right) => left.CompareTo(right) < 0;

    public static bool operator <=(HWND left, HWND right) => left.CompareTo(right) <= 0;

    public static bool operator >(HWND left, HWND right) => left.CompareTo(right) > 0;

    public static bool operator >=(HWND left, HWND right) => left.CompareTo(right) >= 0;

    public bool Equals(HWND other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is HWND w && Equals(w);
    }

    public int CompareTo(HWND other)
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
