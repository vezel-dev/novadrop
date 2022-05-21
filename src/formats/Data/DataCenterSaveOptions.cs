namespace Vezel.Novadrop.Data;

public sealed class DataCenterSaveOptions
{
    public int ClientVersion { get; private set; } = DataCenter.LatestClientVersion;

    public CompressionLevel CompressionLevel { get; private set; }

    public ReadOnlyMemory<byte> Key { get; private set; } = DataCenter.LatestKey;

    public ReadOnlyMemory<byte> IV { get; private set; } = DataCenter.LatestIV;

    DataCenterSaveOptions Clone()
    {
        return new()
        {
            ClientVersion = ClientVersion,
            CompressionLevel = CompressionLevel,
            Key = Key,
            IV = IV,
        };
    }

    public DataCenterSaveOptions WithClientVersion(int clientVersion)
    {
        _ = clientVersion >= DataCenter.LatestClientVersion
            ? true : throw new ArgumentOutOfRangeException(nameof(clientVersion));

        var options = Clone();

        options.ClientVersion = clientVersion;

        return options;
    }

    public DataCenterSaveOptions WithCompressionLevel(CompressionLevel compressionLevel)
    {
        _ = Enum.IsDefined(compressionLevel) ? true : throw new ArgumentOutOfRangeException(nameof(compressionLevel));

        var options = Clone();

        options.CompressionLevel = compressionLevel;

        return options;
    }

    public DataCenterSaveOptions WithKey(ReadOnlySpan<byte> key)
    {
        _ = key.Length == DataCenter.LatestKey.Length ? true : throw new ArgumentException(null, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }

    public DataCenterSaveOptions WithIV(ReadOnlySpan<byte> iv)
    {
        _ = iv.Length == DataCenter.LatestIV.Length ? true : throw new ArgumentException(null, nameof(iv));

        var options = Clone();

        options.IV = iv.ToArray();

        return options;
    }
}
