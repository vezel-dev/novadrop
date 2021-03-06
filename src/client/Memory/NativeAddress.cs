namespace Vezel.Novadrop.Memory;

public readonly struct NativeAddress : IComparable<NativeAddress>, IEquatable<NativeAddress>
{
    public static NativeAddress MinValue { get; } = new(nuint.MinValue);

    public static NativeAddress MaxValue { get; } = new(nuint.MaxValue);

    readonly nuint _value;

    public NativeAddress(nuint value)
    {
        _value = value;
    }

    public static explicit operator NativeAddress(nuint value)
    {
        return new(value);
    }

    public static explicit operator nuint(NativeAddress value)
    {
        return value._value;
    }

    public static bool operator ==(NativeAddress left, NativeAddress right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NativeAddress left, NativeAddress right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(NativeAddress left, NativeAddress right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(NativeAddress left, NativeAddress right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(NativeAddress left, NativeAddress right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(NativeAddress left, NativeAddress right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static NativeAddress operator +(NativeAddress value)
    {
        return new(+value._value);
    }

    public static NativeAddress operator ~(NativeAddress value)
    {
        return new(~value._value);
    }

    public static NativeAddress operator ++(NativeAddress value)
    {
        return new(value._value + 1);
    }

    public static NativeAddress operator --(NativeAddress value)
    {
        return new(value._value - 1);
    }

    public static NativeAddress operator +(NativeAddress left, NativeAddress right)
    {
        return new(left._value + right._value);
    }

    public static NativeAddress operator +(NativeAddress left, nuint right)
    {
        return new(left._value + right);
    }

    public static NativeAddress operator -(NativeAddress left, NativeAddress right)
    {
        return new(left._value - right._value);
    }

    public static NativeAddress operator -(NativeAddress left, nuint right)
    {
        return new(left._value - right);
    }

    public static NativeAddress operator *(NativeAddress left, NativeAddress right)
    {
        return new(left._value * right._value);
    }

    public static NativeAddress operator /(NativeAddress left, NativeAddress right)
    {
        return new(left._value / right._value);
    }

    public static NativeAddress operator %(NativeAddress left, NativeAddress right)
    {
        return new(left._value % right._value);
    }

    public static NativeAddress operator &(NativeAddress left, NativeAddress right)
    {
        return new(left._value & right._value);
    }

    public static NativeAddress operator |(NativeAddress left, NativeAddress right)
    {
        return new(left._value | right._value);
    }

    public static NativeAddress operator ^(NativeAddress left, NativeAddress right)
    {
        return new(left._value ^ right._value);
    }

    public static NativeAddress operator <<(NativeAddress left, int right)
    {
        return new(left._value << right);
    }

    public static NativeAddress operator >>(NativeAddress left, int right)
    {
        return new(left._value >> right);
    }

    public static NativeAddress Clamp(NativeAddress value, NativeAddress min, NativeAddress max)
    {
        return new(Math.Clamp(value._value, min._value, max._value));
    }

    public static NativeAddress Min(NativeAddress val1, NativeAddress val2)
    {
        return new(Math.Min(val1._value, val2._value));
    }

    public static NativeAddress Max(NativeAddress val1, NativeAddress val2)
    {
        return new(Math.Max(val1._value, val2._value));
    }

    public static (NativeAddress Quotient, NativeAddress Remainder) DivRem(NativeAddress left, NativeAddress right)
    {
        var (q, r) = Math.DivRem(left._value, right._value);

        return (new(q), new(r));
    }

    public int CompareTo(NativeAddress other)
    {
        return _value.CompareTo(other._value);
    }

    public bool Equals(NativeAddress other)
    {
        return _value == other._value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is NativeAddress a && Equals(a);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value);
    }

    public override string ToString()
    {
        return $"0x{_value:x}";
    }
}
