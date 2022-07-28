namespace Vezel.Novadrop.Cli;

internal sealed class HexStringConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value is string s
            ? (ReadOnlyMemory<byte>)Convert.FromHexString(s)
            : base.ConvertFrom(context, culture, value);
    }
}
