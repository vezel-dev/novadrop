using Vezel.Novadrop.Data.Nodes;
using Vezel.Novadrop.Data.Serialization;
using Vezel.Novadrop.Data.Serialization.Readers;

namespace Vezel.Novadrop.Data;

public sealed class DataCenter
{
    public static ReadOnlyMemory<byte> LatestEncryptionKey { get; } = new byte[]
    {
        0x33, 0x47, 0xa1, 0x74, 0xf9, 0x04, 0x0d, 0x47,
        0x68, 0xa0, 0xb0, 0x55, 0x58, 0xdc, 0x86, 0x6b,
    };

    public static ReadOnlyMemory<byte> LatestEncryptionIV { get; } = new byte[]
    {
        0xe4, 0x90, 0x56, 0x28, 0x21, 0xaf, 0x3e, 0x11,
        0x76, 0xc9, 0x8d, 0x3c, 0xb9, 0xec, 0x46, 0x01,
    };

    public static int LatestClientVersion => 387463;

    public DataCenterNode Root { get; private set; } = null!;

    DataCenter()
    {
    }

    [SuppressMessage("", "CA5358")]
    internal static Aes CreateCipher(ReadOnlyMemory<byte> key, ReadOnlyMemory<byte> iv)
    {
        var aes = Aes.Create();

        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.Zeros;
        aes.FeedbackSize = 128;
        aes.Key = key.ToArray();
        aes.IV = iv.ToArray();

        return aes;
    }

    public static DataCenter Create()
    {
        var dc = new DataCenter();

        dc.Root = new UserDataCenterNode(dc, DataCenterConstants.RootNodeName);

        return dc;
    }

    public static async Task<DataCenter> LoadAsync(
        Stream stream, DataCenterLoadOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _ = stream.CanRead ? true : throw new ArgumentException(null, nameof(stream));
        ArgumentNullException.ThrowIfNull(options);

        DataCenterReader reader = (options.Mode, options.Mutability) switch
        {
            (DataCenterLoaderMode.Transient, not DataCenterMutability.Mutable) =>
                new TransientDataCenterReader(options),
            (DataCenterLoaderMode.Lazy, DataCenterMutability.Immutable) => new LazyImmutableDataCenterReader(options),
            (DataCenterLoaderMode.Lazy, _) => new LazyMutableDataCenterReader(options),
            (DataCenterLoaderMode.Eager, DataCenterMutability.Immutable) => new EagerImmutableDataCenterReader(options),
            (DataCenterLoaderMode.Eager, _) => new EagerMutableDataCenterReader(options),
            _ => throw new ArgumentException(null, nameof(options)),
        };

        var dc = new DataCenter();

        dc.Root = await reader.ReadAsync(stream, dc, cancellationToken).ConfigureAwait(false);

        return dc;
    }

    public Task SaveAsync(Stream stream, DataCenterSaveOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _ = stream.CanWrite ? true : throw new ArgumentException(null, nameof(stream));
        ArgumentNullException.ThrowIfNull(options);

        return new DataCenterWriter(options).WriteAsync(stream, this, cancellationToken);
    }
}
