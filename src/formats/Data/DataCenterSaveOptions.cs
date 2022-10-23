namespace Vezel.Novadrop.Data;

public sealed class DataCenterSaveOptions
{
    public int Revision { get; private set; } = DataCenter.LatestRevision;

    public CompressionLevel CompressionLevel { get; private set; }

    public ReadOnlyMemory<byte> Key { get; private set; } = DataCenter.LatestKey;

    public ReadOnlyMemory<byte> IV { get; private set; } = DataCenter.LatestIV;

    private DataCenterSaveOptions Clone()
    {
        return new()
        {
            Revision = Revision,
            CompressionLevel = CompressionLevel,
            Key = Key,
            IV = IV,
        };
    }

    public DataCenterSaveOptions WithRevision(int revision)
    {
        Check.Range(revision >= 0, revision);

        var options = Clone();

        options.Revision = revision;

        return options;
    }

    public DataCenterSaveOptions WithCompressionLevel(CompressionLevel compressionLevel)
    {
        Check.Enum(compressionLevel);

        var options = Clone();

        options.CompressionLevel = compressionLevel;

        return options;
    }

    public DataCenterSaveOptions WithKey(scoped ReadOnlySpan<byte> key)
    {
        Check.Argument(key.Length == DataCenter.LatestKey.Length, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }

    public DataCenterSaveOptions WithIV(scoped ReadOnlySpan<byte> iv)
    {
        Check.Argument(iv.Length == DataCenter.LatestIV.Length, nameof(iv));

        var options = Clone();

        options.IV = iv.ToArray();

        return options;
    }
}
