using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Regions;

sealed class DataCenterSegmentedSimpleRegion<T>
    where T : unmanaged, IDataCenterItem
{
    public IReadOnlyList<DataCenterSimpleRegion<T>> Segments { get; }

    public DataCenterSegmentedSimpleRegion(int count)
    {
        var segs = new List<DataCenterSimpleRegion<T>>(count);

        for (var i = 0; i < count; i++)
            segs.Add(new(false));

        Segments = segs;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        foreach (var region in Segments)
            await region.ReadAsync(reader, cancellationToken).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
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
