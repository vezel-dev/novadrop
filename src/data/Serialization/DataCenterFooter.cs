using Vezel.Novadrop.Data.IO;

namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterFooter
{
    public int Marker { get; private set; }

    public async ValueTask ReadAsync(bool strict, DataCenterBinaryReader reader, CancellationToken cancellationToken)
    {
        Marker = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (strict && Marker != 0)
            throw new InvalidDataException($"Unexpected data center footer marker {Marker}.");
    }

    public async ValueTask WriteAsync(DataCenterBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(Marker, cancellationToken).ConfigureAwait(false);
    }
}
