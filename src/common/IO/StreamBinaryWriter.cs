namespace Vezel.Novadrop.IO;

internal sealed class StreamBinaryWriter
{
    private readonly Stream _stream;

    private readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(ulong));

    public StreamBinaryWriter(Stream stream)
    {
        _stream = stream;
    }

    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        return _stream.WriteAsync(buffer, cancellationToken);
    }

    public ValueTask WriteByteAsync(byte value, CancellationToken cancellationToken)
    {
        _buffer.Span[0] = value;

        return WriteAsync(_buffer[..sizeof(byte)], cancellationToken);
    }

    public ValueTask WriteSByteAsync(sbyte value, CancellationToken cancellationToken)
    {
        return WriteByteAsync((byte)value, cancellationToken);
    }

    public ValueTask WriteUInt16Async(ushort value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span, value);

        return WriteAsync(_buffer[..sizeof(ushort)], cancellationToken);
    }

    public ValueTask WriteInt16Async(short value, CancellationToken cancellationToken)
    {
        return WriteUInt16Async((ushort)value, cancellationToken);
    }

    public ValueTask WriteUInt32Async(uint value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Span, value);

        return WriteAsync(_buffer[..sizeof(uint)], cancellationToken);
    }

    public ValueTask WriteInt32Async(int value, CancellationToken cancellationToken)
    {
        return WriteUInt32Async((uint)value, cancellationToken);
    }

    public ValueTask WriteUInt64Async(ulong value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Span, value);

        return WriteAsync(_buffer[..sizeof(ulong)], cancellationToken);
    }

    public ValueTask WriteInt64Async(long value, CancellationToken cancellationToken)
    {
        return WriteUInt64Async((ulong)value, cancellationToken);
    }

    public ValueTask WriteSingleAsync(float value, CancellationToken cancellationToken)
    {
        return WriteUInt32Async(Unsafe.As<float, uint>(ref value), cancellationToken);
    }

    public ValueTask WriteDoubleAsync(double value, CancellationToken cancellationToken)
    {
        return WriteUInt64Async(Unsafe.As<double, ulong>(ref value), cancellationToken);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask WriteStringAsync(string value, CancellationToken cancellationToken)
    {
        foreach (var c in value)
            await WriteUInt16Async(c, cancellationToken).ConfigureAwait(false);

        await WriteUInt16Async(0, cancellationToken).ConfigureAwait(false);
    }
}
