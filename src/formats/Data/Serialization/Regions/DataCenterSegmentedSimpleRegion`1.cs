// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Regions;

internal sealed class DataCenterSegmentedSimpleRegion<T>
    where T : struct, IDataCenterItem<T>
{
    public IReadOnlyList<DataCenterSimpleRegion<T>> Segments { get; }

    public DataCenterSegmentedSimpleRegion(int count)
    {
        var segs = new List<DataCenterSimpleRegion<T>>(count);

        for (var i = 0; i < count; i++)
            segs.Add(new(offByOne: false));

        Segments = segs;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(
        DataCenterArchitecture architecture, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        foreach (var region in Segments)
            await region.ReadAsync(architecture, reader, cancellationToken).ConfigureAwait(false);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(
        DataCenterArchitecture architecture, StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        foreach (var region in Segments)
            await region.WriteAsync(architecture, writer, cancellationToken).ConfigureAwait(false);
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
