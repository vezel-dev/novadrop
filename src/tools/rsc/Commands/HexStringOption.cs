namespace Vezel.Novadrop.Commands;

class HexStringOption : Option<ReadOnlyMemory<byte>>
{
    public HexStringOption(string name, ReadOnlyMemory<byte> defaultValue, string description)
        : base(name, ParseArgument, true, description)
    {
        SetDefaultValue(defaultValue);
    }

    static ReadOnlyMemory<byte> ParseArgument(ArgumentResult result)
    {
        var value = result.Tokens[0].Value;

        try
        {
            return Convert.FromHexString(value);
        }
        catch (FormatException ex)
        {
            result.ErrorMessage = $"'{value}': {ex.Message}";

            return default;
        }
    }
}
