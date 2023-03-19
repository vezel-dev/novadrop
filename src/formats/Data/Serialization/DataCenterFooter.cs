namespace Vezel.Novadrop.Data.Serialization;

sealed class DataCenterFooter
{
    public int Marker { get; private set; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        Marker = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (strict && Marker != 0)
            throw new InvalidDataException($"Unexpected data center footer marker {Marker}.");
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(Marker, cancellationToken).ConfigureAwait(false);
    }
}
