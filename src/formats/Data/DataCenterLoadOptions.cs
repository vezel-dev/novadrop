namespace Vezel.Novadrop.Data;

public sealed class DataCenterLoadOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = DataCenter.LatestKey;

    public ReadOnlyMemory<byte> IV { get; private set; } = DataCenter.LatestIV;

    public DataCenterArchitecture Architecture { get; private set; } = DataCenterArchitecture.X64;

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
            Architecture = Architecture,
            Mode = Mode,
            Mutability = Mutability,
        };
    }

    public DataCenterLoadOptions WithKey(scoped ReadOnlySpan<byte> key)
    {
        Check.Argument(key.Length == DataCenter.LatestKey.Length, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }

    public DataCenterLoadOptions WithIV(scoped ReadOnlySpan<byte> iv)
    {
        Check.Argument(iv.Length == DataCenter.LatestIV.Length, nameof(iv));

        var options = Clone();

        options.IV = iv.ToArray();

        return options;
    }

    public DataCenterLoadOptions WithArchitecture(DataCenterArchitecture architecture)
    {
        Check.Enum(architecture);

        var options = Clone();

        options.Architecture = architecture;

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
        Check.Enum(mode);

        var options = Clone();

        options.Mode = mode;

        return options;
    }

    public DataCenterLoadOptions WithMutability(DataCenterMutability mutability)
    {
        Check.Enum(mutability);

        var options = Clone();

        options.Mutability = mutability;

        return options;
    }
}
