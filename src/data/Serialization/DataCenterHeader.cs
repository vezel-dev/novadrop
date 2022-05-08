using Vezel.Novadrop.Data.IO;

namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterHeader
{
    const int KnownFormatVersion = 6;

    public int FormatVersion { get; private set; }

    public int Unknown1 { get; private set; }

    public ushort Unknown2 { get; private set; }

    public ushort Unknown3 { get; private set; }

    public int ClientVersion { get; set; }

    public int Unknown4 { get; private set; }

    public int Unknown5 { get; private set; }

    public int Unknown6 { get; private set; }

    public int Unknown7 { get; private set; }

    public async ValueTask ReadAsync(bool strict, DataCenterBinaryReader reader, CancellationToken cancellationToken)
    {
        FormatVersion = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown1 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown2 = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
        Unknown3 = await reader.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
        ClientVersion = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown4 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown5 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown6 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        Unknown7 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (FormatVersion != KnownFormatVersion)
            throw new InvalidDataException(
                $"Unsupported data center format version {FormatVersion} (expected {KnownFormatVersion}).");

        var tup = (Unknown1, Unknown2, Unknown3, Unknown4, Unknown5, Unknown6, Unknown7);

        if (strict && tup != (0, 0, 0xbff0, 0, 0, 0, 0))
            throw new InvalidDataException($"Unexpected data center header values {tup}.");
    }

    public async ValueTask WriteAsync(DataCenterBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(KnownFormatVersion, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteUInt16Async(0, cancellationToken).ConfigureAwait(false);

        // TODO: What is this value?
        await writer.WriteUInt16Async(0xbff0, cancellationToken).ConfigureAwait(false);

        await writer.WriteInt32Async(ClientVersion, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
        await writer.WriteInt32Async(0, cancellationToken).ConfigureAwait(false);
    }
}
