using Vezel.Novadrop.Data.IO;

namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterFooter
{
    public int Unknown1 { get; set; }

    public async ValueTask ReadAsync(bool strict, DataCenterBinaryReader reader, CancellationToken cancellationToken)
    {
        Unknown1 = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (strict && Unknown1 != 0)
            throw new InvalidDataException($"Unexpected data center footer value {Unknown1}.");
    }

    public async ValueTask WriteAsync(DataCenterBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(Unknown1, cancellationToken).ConfigureAwait(false);
    }
}
