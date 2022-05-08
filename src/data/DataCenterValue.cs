namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct DataCenterValue : IEquatable<DataCenterValue>, IComparable<DataCenterValue>
{
    public DataCenterTypeCode TypeCode { get; }

    public bool IsNull => TypeCode == DataCenterTypeCode.None;

    public bool IsInt32 => TypeCode == DataCenterTypeCode.Int32;

    public bool IsSingle => TypeCode == DataCenterTypeCode.Single;

    public bool IsString => TypeCode == DataCenterTypeCode.String;

    public bool IsBoolean => TypeCode == DataCenterTypeCode.Boolean;

    public object Value
    {
        get
        {
            return TypeCode switch
            {
                DataCenterTypeCode.Int32 => As<int>(),
                DataCenterTypeCode.Single => As<float>(),
                DataCenterTypeCode.String => _stringValue!,
                DataCenterTypeCode.Boolean => As<bool>(),
                _ => throw new InvalidOperationException("Value is null."),
            };
        }
    }

    public int AsInt32 =>
        IsInt32
            ? _primitiveValue
            : throw new InvalidOperationException($"Value is not of type {DataCenterTypeCode.Int32}.");

    public float AsSingle =>
        IsSingle
            ? As<float>()
            : throw new InvalidOperationException($"Value is not of type {DataCenterTypeCode.Single}.");

    public string AsString =>
        IsString
            ? _stringValue!
            : throw new InvalidOperationException($"Value is not of type {DataCenterTypeCode.String}.");

    public bool AsBoolean =>
        IsBoolean
            ? As<bool>()
            : throw new InvalidOperationException($"Value is not of type {DataCenterTypeCode.Boolean}.");

    readonly int _primitiveValue;

    readonly string? _stringValue;

    DataCenterValue(DataCenterTypeCode typeCode, int primitiveValue, string? stringValue)
    {
        TypeCode = typeCode;
        _primitiveValue = primitiveValue;
        _stringValue = stringValue;
    }

    public DataCenterValue(int value)
        : this(DataCenterTypeCode.Int32, value, null)
    {
    }

    public DataCenterValue(float value)
        : this(DataCenterTypeCode.Single, Unsafe.As<float, int>(ref value), null)
    {
    }

    public DataCenterValue(string value)
        : this(DataCenterTypeCode.String, 0, value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }

    public DataCenterValue(bool value)
        : this(DataCenterTypeCode.Boolean, value ? 1 : 0, null)
    {
    }

    public static implicit operator DataCenterValue(int value)
    {
        return new(value);
    }

    public static implicit operator DataCenterValue(float value)
    {
        return new(value);
    }

    public static implicit operator DataCenterValue(string value)
    {
        return new(value);
    }

    public static implicit operator DataCenterValue(bool value)
    {
        return new(value);
    }

    public static explicit operator int(DataCenterValue value)
    {
        return value.AsInt32;
    }

    public static explicit operator float(DataCenterValue value)
    {
        return value.AsSingle;
    }

    public static explicit operator string(DataCenterValue value)
    {
        return value.AsString;
    }

    public static explicit operator bool(DataCenterValue value)
    {
        return value.AsBoolean;
    }

    public static bool operator ==(DataCenterValue left, DataCenterValue right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DataCenterValue left, DataCenterValue right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(DataCenterValue left, DataCenterValue right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(DataCenterValue left, DataCenterValue right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(DataCenterValue left, DataCenterValue right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(DataCenterValue left, DataCenterValue right)
    {
        return left.CompareTo(right) >= 0;
    }

    T As<T>()
        where T : unmanaged
    {
        return Unsafe.As<int, T>(ref Unsafe.AsRef(_primitiveValue));
    }

    public bool Equals(DataCenterValue other)
    {
        return TypeCode == other.TypeCode && TypeCode switch
        {
            DataCenterTypeCode.Int32 => As<int>() == other.As<int>(),
            DataCenterTypeCode.Single => As<float>() == other.As<float>(),
            DataCenterTypeCode.String => _stringValue == other._stringValue,
            DataCenterTypeCode.Boolean => As<bool>() == other.As<bool>(),
            _ => true,
        };
    }

    public int CompareTo(DataCenterValue other)
    {
        return TypeCode != other.TypeCode ? TypeCode.CompareTo(other.TypeCode) : TypeCode switch
        {
            DataCenterTypeCode.Int32 => As<int>().CompareTo(other.As<int>()),
            DataCenterTypeCode.Single => As<float>().CompareTo(other.As<float>()),
            DataCenterTypeCode.String => string.CompareOrdinal(_stringValue, other._stringValue),
            DataCenterTypeCode.Boolean => As<bool>().CompareTo(other.As<bool>()),
            _ => 0,
        };
    }

    public int ToInt32(int? fallback = null)
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => As<int>(),
            DataCenterTypeCode.Single => (int)As<float>(),
            DataCenterTypeCode.String =>
                int.TryParse(_stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                    ? i
                    : fallback ?? throw new InvalidCastException(),
            var t => throw new InvalidCastException($"Cannot cast value of type {t} to {typeof(int)}."),
        };
    }

    public float ToSingle(float? fallback = null)
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => As<int>(),
            DataCenterTypeCode.Single => As<float>(),
            DataCenterTypeCode.String =>
                float.TryParse(_stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
                    ? f
                    : fallback ?? throw new InvalidCastException(),
            var t => throw new InvalidCastException($"Cannot cast value of type {t} to {typeof(float)}."),
        };
    }

    public bool ToBoolean(bool? fallback = null)
    {
        return TypeCode switch
        {
            DataCenterTypeCode.String =>
                bool.TryParse(_stringValue, out var b)
                    ? b
                    : fallback ?? throw new InvalidCastException(),
            DataCenterTypeCode.Boolean => As<bool>(),
            var t => throw new InvalidCastException($"Cannot cast value of type {t} to {typeof(bool)}."),
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is DataCenterValue v && Equals(v);
    }

    public override int GetHashCode()
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => HashCode.Combine(As<int>()),
            DataCenterTypeCode.Single => HashCode.Combine(As<float>()),
            DataCenterTypeCode.String => HashCode.Combine(_stringValue),
            DataCenterTypeCode.Boolean => HashCode.Combine(As<bool>()),
            _ => 0,
        };
    }

    public override string ToString()
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => As<int>().ToString(CultureInfo.InvariantCulture),
            DataCenterTypeCode.Single => As<float>().ToString(CultureInfo.InvariantCulture),
            DataCenterTypeCode.String => _stringValue!,
            DataCenterTypeCode.Boolean => As<bool>().ToString(),
            _ => "null",
        };
    }
}
