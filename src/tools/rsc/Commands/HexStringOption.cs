namespace Vezel.Novadrop.Commands;

class HexStringOption : Option<ReadOnlyMemory<byte>>
{
    public HexStringOption(string name, ReadOnlyMemory<byte> defaultValue, string description)
        : base(name, result => ParseArgument(result, defaultValue), true, description)
    {
        SetDefaultValue(defaultValue);
    }

    static ReadOnlyMemory<byte> ParseArgument(ArgumentResult result, ReadOnlyMemory<byte> defaultValue)
    {
        var value = result.Tokens[0].Value;
        var array = default(ReadOnlyMemory<byte>);

        try
        {
            array = Convert.FromHexString(value);
        }
        catch (FormatException ex)
        {
            result.ErrorMessage = $"'{value}': {ex.Message}";
        }

        if (array.Length != defaultValue.Length)
            result.ErrorMessage = $"'{value}': The input hex string must be {defaultValue.Length} bytes long.";

        return array;
    }
}
