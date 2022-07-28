namespace Vezel.Novadrop.Data;

public sealed class DataCenterLoadOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = DataCenter.LatestKey;

    public ReadOnlyMemory<byte> IV { get; private set; } = DataCenter.LatestIV;

    public bool Strict { get; private set; }

    public DataCenterLoaderMode Mode { get; private set; }

    public DataCenterMutability Mutability { get; private set; }

    private DataCenterLoadOptions Clone()
    {
        return new()
        {
            Key = Key,
            IV = IV,
            Strict = Strict,
            Mode = Mode,
            Mutability = Mutability,
        };
    }

    public DataCenterLoadOptions WithKey(ReadOnlySpan<byte> key)
    {
        _ = key.Length == DataCenter.LatestKey.Length ? true : throw new ArgumentException(null, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }

    public DataCenterLoadOptions WithIV(ReadOnlySpan<byte> iv)
    {
        _ = iv.Length == DataCenter.LatestIV.Length ? true : throw new ArgumentException(null, nameof(iv));

        var options = Clone();

        options.IV = iv.ToArray();

        return options;
    }

    public DataCenterLoadOptions WithStrict(bool strict)
    {
        var options = Clone();

        options.Strict = strict;

        return options;
    }

    public DataCenterLoadOptions WithLoaderMode(DataCenterLoaderMode mode)
    {
        _ = Enum.IsDefined(mode) ? true : throw new ArgumentOutOfRangeException(nameof(mode));

        var options = Clone();

        options.Mode = mode;

        return options;
    }

    public DataCenterLoadOptions WithMutability(DataCenterMutability mutability)
    {
        _ = Enum.IsDefined(mutability) ? true : throw new ArgumentOutOfRangeException(nameof(mutability));

        var options = Clone();

        options.Mutability = mutability;

        return options;
    }
}
