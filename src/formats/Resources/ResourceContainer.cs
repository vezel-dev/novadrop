using Vezel.Novadrop.Cryptography;

namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainer
{
    const int FooterLength = sizeof(int) * 2;

    const uint Magic = 0x01001fff;

    public static ReadOnlyMemory<byte> LatestKey { get; } = new byte[]
    {
        0x1c, 0x24, 0x00, 0x00, 0x1f, 0x04, 0x00, 0x00,
        0x72, 0xf4, 0x00, 0x00, 0x1d, 0x62, 0x00, 0x00,
        0xbd, 0xa8, 0x00, 0x00, 0xdb, 0xa7, 0x00, 0x00,
        0x01, 0x30, 0x00, 0x00, 0x33, 0x27, 0x00, 0x00,
    };

    public IReadOnlyDictionary<string, ResourceContainerEntry> Entries => _entries;

    readonly Dictionary<string, ResourceContainerEntry> _entries = new();

    ResourceContainer()
    {
    }

    [SuppressMessage("", "CA5358")]
    internal static Xor256 CreateCipher(ReadOnlyMemory<byte> key)
    {
        var xor = Xor256.Create();

        xor.Key = key.ToArray();

        return xor;
    }

    public static ResourceContainer Create()
    {
        return new();
    }

    public static async Task<ResourceContainer> LoadAsync(
        Stream stream, ResourceContainerLoadOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _ = stream is { CanRead: true, CanSeek: true } ? true : throw new ArgumentException(null, nameof(stream));
        ArgumentNullException.ThrowIfNull(options);

        var rc = new ResourceContainer();

        await Task.Run(
            async () =>
            {
                stream.Position = stream.Length - FooterLength;

                var footerReader = new StreamBinaryReader(stream);

                var dirSize = await footerReader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
                var magic = await footerReader.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                if (dirSize < 0)
                    throw new InvalidDataException($"Directory size {dirSize} is negative.");

                if (magic != Magic)
                    throw new InvalidDataException(
                        $"Unsupported resource container magic value 0x{magic:x8} (expected 0x{Magic:x8}).");

                stream.Position = stream.Length - FooterLength - dirSize;

                using var xor = CreateCipher(options.Key);
                using var decryptor = xor.CreateDecryptor();

                var dirStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true);
                var entries = new List<(string, int, int)>();

                await using (dirStream.ConfigureAwait(false))
                {
                    var dirReader = new StreamBinaryReader(dirStream);

                    while (dirReader.Progress < dirSize)
                    {
                        var length = await dirReader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
                        var offset = await dirReader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
                        var name = await dirReader.ReadStringAsync(cancellationToken).ConfigureAwait(false);

                        if (offset < 0)
                            throw new InvalidDataException($"Entry offset {offset} is negative.");

                        if (length < 0)
                            throw new InvalidDataException($"Entry length {length} is negative.");

                        entries.Add((name, offset, length));
                    }

                    if (options.Strict && dirReader.Progress != dirSize)
                        throw new InvalidDataException(
                            $"Directory size {dirSize} does not match actual size {dirReader.Progress}.");
                }

                foreach (var (name, offset, length) in entries)
                {
                    stream.Position = offset;

                    var entryStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read, true);
                    var data = GC.AllocateUninitializedArray<byte>(length);

                    await using (entryStream.ConfigureAwait(false))
                        await new StreamBinaryReader(entryStream)
                            .ReadAsync(data, cancellationToken)
                            .ConfigureAwait(false);

                    var entry = new ResourceContainerEntry(rc, name)
                    {
                        Data = data,
                    };

                    if (!rc._entries.TryAdd(name, entry))
                        throw new InvalidDataException($"Entry named '{name}' was already recorded earlier.");
                }

                stream.Position = stream.Length;
            },
            cancellationToken).ConfigureAwait(false);

        return rc;
    }

    public Task SaveAsync(
        Stream stream, ResourceContainerSaveOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _ = stream.CanWrite ? true : throw new ArgumentException(null, nameof(stream));
        ArgumentNullException.ThrowIfNull(options);

        return Task.Run(
            async () =>
            {
                var entries = new List<(string, int, int)>(_entries.Count);

                using var xor = CreateCipher(options.Key);
                using var encryptor = xor.CreateEncryptor();

                foreach (var (name, entry) in _entries)
                {
                    var entryStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write, true);
                    var entryWriter = new StreamBinaryWriter(entryStream);

                    await using (entryStream.ConfigureAwait(false))
                    {
                        var data = entry.Data;

                        entries.Add((name, (int)stream.Position, data.Length));

                        await entryWriter.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                    }
                }

                var dirStart = stream.Position;

                var dirStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write, true);
                var dirWriter = new StreamBinaryWriter(dirStream);

                await using (dirStream.ConfigureAwait(false))
                {
                    foreach (var (name, offset, length) in entries)
                    {
                        await dirWriter.WriteInt32Async(length, cancellationToken).ConfigureAwait(false);
                        await dirWriter.WriteInt32Async(offset, cancellationToken).ConfigureAwait(false);
                        await dirWriter.WriteStringAsync(name, cancellationToken).ConfigureAwait(false);
                    }
                }

                var footerWriter = new StreamBinaryWriter(stream);

                await footerWriter
                    .WriteInt32Async((int)(stream.Position - dirStart), cancellationToken)
                    .ConfigureAwait(false);
                await footerWriter.WriteUInt32Async(Magic, cancellationToken).ConfigureAwait(false);

                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            },
            cancellationToken);
    }

    public ResourceContainerEntry CreateEntry(string name)
    {
        var entry = new ResourceContainerEntry(this, name);

        _entries.Add(name, entry);

        return entry;
    }

    public bool RemoveEntry(string name)
    {
        return _entries.Remove(name);
    }
}
