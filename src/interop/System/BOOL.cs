namespace Vezel.Novadrop.Interop.System;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct BOOL :
    IEquatable<BOOL>,
    IEqualityOperators<BOOL, BOOL, bool>,
    IComparable<BOOL>,
    IComparisonOperators<BOOL, BOOL, bool>
{
    public static BOOL TRUE { get; } = new(1);

    public static BOOL FALSE { get; } = new(0);

    private readonly int _value;

    public BOOL(int value)
    {
        _value = value;
    }

    public static bool operator true(BOOL value) => value;

    public static bool operator false(BOOL value) => !value;

    public static implicit operator BOOL(bool value) => new(value ? 1 : 0);

    public static implicit operator bool(BOOL value) => value._value != 0;

    public static bool operator ==(BOOL left, BOOL right) => left.Equals(right);

    public static bool operator !=(BOOL left, BOOL right) => !left.Equals(right);

    public static bool operator <(BOOL left, BOOL right) => left.CompareTo(right) < 0;

    public static bool operator <=(BOOL left, BOOL right) => left.CompareTo(right) <= 0;

    public static bool operator >(BOOL left, BOOL right) => left.CompareTo(right) > 0;

    public static bool operator >=(BOOL left, BOOL right) => left.CompareTo(right) >= 0;

    public bool Equals(BOOL other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is BOOL b && Equals(b);
    }

    public int CompareTo(BOOL other)
    {
        return _value.CompareTo(other._value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value);
    }

    public override string ToString()
    {
        return this ? "TRUE" : "FALSE";
    }
}
