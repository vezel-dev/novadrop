using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Regions;

sealed class DataCenterSegmentedRegion<T>
    where T : unmanaged, IDataCenterItem
{
    public List<DataCenterRegion<T>> Segments { get; } = new List<DataCenterRegion<T>>(ushort.MaxValue);

    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        var count = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        if (count < 0)
            throw new InvalidDataException($"Region segment count {count} is negative.");

        for (var i = 0; i < count; i++)
        {
            var region = new DataCenterRegion<T>();

            await region.ReadAsync(strict, reader, cancellationToken).ConfigureAwait(false);

            Segments.Add(region);
        }
    }

    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        await writer.WriteInt32Async(Segments.Count, cancellationToken).ConfigureAwait(false);

        foreach (var region in Segments)
            await region.WriteAsync(writer, cancellationToken).ConfigureAwait(false);
    }

    public T GetElement(DataCenterAddress address)
    {
        return address.SegmentIndex < Segments.Count
            ? Segments[address.SegmentIndex].GetElement(address.ElementIndex)
            : throw new InvalidDataException(
                $"Region segment index {address.SegmentIndex} is out of bounds (0..{Segments.Count}).");
    }

    public void SetElement(DataCenterAddress address, T value)
    {
        Segments[address.SegmentIndex].SetElement(address.ElementIndex, value);
    }
}
