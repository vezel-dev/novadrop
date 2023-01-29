namespace Vezel.Novadrop.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct DataCenterValue :
    IEquatable<DataCenterValue>,
    IEqualityOperators<DataCenterValue, DataCenterValue, bool>,
    IComparable<DataCenterValue>,
    IComparisonOperators<DataCenterValue, DataCenterValue, bool>
{
    public DataCenterTypeCode TypeCode { get; }

    public bool IsNull => TypeCode == DataCenterTypeCode.Null;

    public bool IsInt32 => TypeCode == DataCenterTypeCode.Int32;

    public bool IsSingle => TypeCode == DataCenterTypeCode.Single;

    public bool IsString => TypeCode == DataCenterTypeCode.String;

    public bool IsBoolean => TypeCode == DataCenterTypeCode.Boolean;

    public object? Value => TypeCode switch
    {
        DataCenterTypeCode.Int32 => UnsafeAsInt32,
        DataCenterTypeCode.Single => UnsafeAsSingle,
        DataCenterTypeCode.String => UnsafeAsString,
        DataCenterTypeCode.Boolean => UnsafeAsBoolean,
        _ => null,
    };

    public int AsInt32
    {
        get
        {
            Check.Operation(IsInt32, $"Value is not of type {DataCenterTypeCode.Int32}.");

            return UnsafeAsInt32;
        }
    }

    public float AsSingle
    {
        get
        {
            Check.Operation(IsSingle, $"Value is not of type {DataCenterTypeCode.Single}.");

            return UnsafeAsSingle;
        }
    }

    public string AsString
    {
        get
        {
            Check.Operation(IsString, $"Value is not of type {DataCenterTypeCode.String}.");

            return UnsafeAsString;
        }
    }

    public bool AsBoolean
    {
        get
        {
            Check.Operation(IsBoolean, $"Value is not of type {DataCenterTypeCode.Boolean}.");

            return UnsafeAsBoolean;
        }
    }

    [SuppressMessage("", "IDE0032")]
    internal int UnsafeAsInt32 => _primitiveValue;

    internal float UnsafeAsSingle => Unsafe.As<int, float>(ref Unsafe.AsRef(_primitiveValue));

    internal string UnsafeAsString => _stringValue!;

    internal bool UnsafeAsBoolean => Unsafe.As<int, bool>(ref Unsafe.AsRef(_primitiveValue));

    [SuppressMessage("", "IDE0032")]
    private readonly int _primitiveValue;

    private readonly string? _stringValue;

    private DataCenterValue(DataCenterTypeCode typeCode, int primitiveValue, string? stringValue)
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
        Check.Null(value);
    }

    public DataCenterValue(bool value)
        : this(DataCenterTypeCode.Boolean, value ? 1 : 0, null)
    {
    }

    public static implicit operator DataCenterValue(int value) => new(value);

    public static implicit operator DataCenterValue(float value) => new(value);

    public static implicit operator DataCenterValue(string value) => new(value);

    public static implicit operator DataCenterValue(bool value) => new(value);

    public static explicit operator int(DataCenterValue value) => value.AsInt32;

    public static explicit operator float(DataCenterValue value) => value.AsSingle;

    public static explicit operator string(DataCenterValue value) => value.AsString;

    public static explicit operator bool(DataCenterValue value) => value.AsBoolean;

    public static bool operator ==(DataCenterValue left, DataCenterValue right) => left.Equals(right);

    public static bool operator !=(DataCenterValue left, DataCenterValue right) => !left.Equals(right);

    public static bool operator <(DataCenterValue left, DataCenterValue right) => left.CompareTo(right) < 0;

    public static bool operator <=(DataCenterValue left, DataCenterValue right) => left.CompareTo(right) <= 0;

    public static bool operator >(DataCenterValue left, DataCenterValue right) => left.CompareTo(right) > 0;

    public static bool operator >=(DataCenterValue left, DataCenterValue right) => left.CompareTo(right) >= 0;

    public int CompareTo(DataCenterValue other)
    {
        return TypeCode != other.TypeCode ? TypeCode.CompareTo(other.TypeCode) : TypeCode switch
        {
            DataCenterTypeCode.Int32 => UnsafeAsInt32.CompareTo(other.UnsafeAsInt32),
            DataCenterTypeCode.Single => UnsafeAsSingle.CompareTo(other.UnsafeAsSingle),
            DataCenterTypeCode.String => string.CompareOrdinal(UnsafeAsString, other.UnsafeAsString),
            DataCenterTypeCode.Boolean => UnsafeAsBoolean.CompareTo(other.UnsafeAsBoolean),
            _ => 0,
        };
    }

    public bool Equals(DataCenterValue other)
    {
        return TypeCode == other.TypeCode && TypeCode switch
        {
            DataCenterTypeCode.Int32 => UnsafeAsInt32 == other.UnsafeAsInt32,
            DataCenterTypeCode.Single => UnsafeAsSingle == other.UnsafeAsSingle,
            DataCenterTypeCode.String => UnsafeAsString == other.UnsafeAsString,
            DataCenterTypeCode.Boolean => UnsafeAsBoolean == other.UnsafeAsBoolean,
            _ => true,
        };
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is DataCenterValue v && Equals(v);
    }

    public override int GetHashCode()
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => HashCode.Combine(UnsafeAsInt32),
            DataCenterTypeCode.Single => HashCode.Combine(UnsafeAsSingle),
            DataCenterTypeCode.String => HashCode.Combine(UnsafeAsString),
            DataCenterTypeCode.Boolean => HashCode.Combine(UnsafeAsBoolean),
            _ => 0,
        };
    }

    public override string ToString()
    {
        return TypeCode switch
        {
            DataCenterTypeCode.Int32 => UnsafeAsInt32.ToString(CultureInfo.InvariantCulture),
            DataCenterTypeCode.Single => UnsafeAsSingle.ToString(CultureInfo.InvariantCulture),
            DataCenterTypeCode.String => UnsafeAsString,
            DataCenterTypeCode.Boolean => UnsafeAsBoolean ? "true" : "false", // Avoid "True" and "False".
            _ => "null",
        };
    }
}
