namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterHeader
{
    const int KnownVersion = 6;

    const double KnownTimestamp = -1.0;

    public int Version { get; private set; }

    public double Timestamp { get; private set; }

    public int Revision { get; set; }

    public int Unknown4 { get; private set; }

    public int Unknown5 { get; private set; }

    public int Unknown6 { get; private set; }

    public int Unknown7 { get; private set; }

    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        Version = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Timestamp = await reader.ReadDoubleAsync(cancellationToken).ConfigureAwait(false);
        Revision = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown4 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown5 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown6 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown7 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (Version != KnownVersion)
            throw new InvalidDataException(
                $"Unsupported data center version {Version} (expected {KnownVersion}).");

        if (Timestamp != KnownTimestamp)
            throw new InvalidDataException(
                $"Unexpected data center timestamp {Timestamp} (expected {KnownTimestamp}).");

        var tup = (Unknown4, Unknown5, Unknown6, Unknown7);

        if (strict && tup != (0, 0, 0, 0))
            throw new InvalidDataException($"Unexpected data center type tree values {tup}.");
    }

    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(KnownVersion, cancellationToken).ConfigureAwait(false);
        await writer.WriteDoubleAsync(KnownTimestamp, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(Revision, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
    }
}
