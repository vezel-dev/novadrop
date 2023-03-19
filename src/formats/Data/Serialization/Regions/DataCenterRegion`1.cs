using Vezel.Novadrop.Data.Serialization.Items;

namespace Vezel.Novadrop.Data.Serialization.Regions;

internal sealed class DataCenterRegion<T>
    where T : unmanaged, IDataCenterItem
{
    public List<T> Elements { get; } = new(ushort.MaxValue);

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask ReadAsync(bool strict, StreamBinaryReader reader, CancellationToken cancellationToken)
    {
        var capacity = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);
        var count = await reader.ReadInt32Async(cancellationToken).ConfigureAwait(false);

        Check.Data(count >= 0, $"Region length {count} is negative.");

        if (strict)
        {
            Check.Data(capacity >= 0, $"Region capacity {capacity} is negative.");
            Check.Data(count <= capacity, $"Region length {count} is greater than region capacity {capacity}.");
        }

        var length = Unsafe.SizeOf<T>() * capacity;
        var bytes = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            await reader.ReadAsync(bytes.AsMemory(0, length), cancellationToken).ConfigureAwait(false);

            void ProcessElements()
            {
                foreach (ref var elem in MemoryMarshal.CreateSpan(
                    ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(bytes)), count))
                {
                    if (!BitConverter.IsLittleEndian)
                        elem.ReverseEndianness();

                    Elements.Add(elem);
                }
            }

            // Cannot use refs in async methods...
            ProcessElements();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteAsync(StreamBinaryWriter writer, CancellationToken cancellationToken)
    {
        var count = Elements.Count;

        for (var i = 0; i < 2; i++)
            await writer.WriteInt32Async(count, cancellationToken).ConfigureAwait(false);

        var length = Unsafe.SizeOf<T>() * count;
        var bytes = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            void ProcessElements()
            {
                var i = 0;

                foreach (ref var elem in MemoryMarshal.CreateSpan(
                    ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(bytes)), count))
                {
                    elem = Elements[i++];

                    if (!BitConverter.IsLittleEndian)
                        elem.ReverseEndianness();
                }
            }

            // Cannot use refs in async methods...
            ProcessElements();

            await writer.WriteAsync(bytes.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    public T GetElement(int index)
    {
        Check.Data(index < Elements.Count, $"Region element index {index} is out of bounds (0..{Elements.Count}).");

        return Elements[index];
    }

    public void SetElement(int index, T value)
    {
        Elements[index] = value;
    }
}
