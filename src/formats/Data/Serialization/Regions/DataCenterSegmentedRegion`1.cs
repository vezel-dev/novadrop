using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Regions;

internal sealed class DataCenterSegmentedRegion<T>
    where T : unmanaged, IDataCenterItem
{
    public List<DataCenterRegion<T>> Segments { get; } = new(ushort.MaxValue);

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        var count = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        Check.Data(count >= 0, $"Region segment count {count} is negative.");

        for (var i = 0; i < count; i++)
        {
            var region = new DataCenterRegion<T>();

            await region.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);

            Segments.Add(region);
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(Segments.Count, cancellationToken).ConfigureAwait(false);

        foreach (var region in Segments)
            await region.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
    }

    public T GetElement(DataCenterAddress address)
    {
        Check.Data(
            address.SegmentIndex < Segments.Count,
            $"Region segment index {address.SegmentIndex} is out of bounds (0..{Segments.Count}).");

        return Segments[address.SegmentIndex].GetElement(address.ElementIndex);
    }

    public void SetElement(DataCenterAddress address, T value)
    {
        Segments[address.SegmentIndex].SetElement(address.ElementIndex, value);
    }
}
