namespace Vezel.Novadrop.IO;

sealed class StreamBinaryWriter
{
    readonly Stream _stream;

    readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(double));

    public StreamBinaryWriter(Stream stream)
    {
        _stream = stream;
    }

    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        await _stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask WriteUInt16Async(ushort value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Span, value);

        await WriteAsync(_buffer[..sizeof(ushort)], cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask WriteUInt32Async(uint value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Span, value);

        await WriteAsync(_buffer[..sizeof(uint)], cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask WriteInt32Async(int value, CancellationToken cancellationToken)
    {
        await WriteUInt32Async((uint)value, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask WriteDoubleAsync(double value, CancellationToken cancellationToken)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.Span, value);

        await WriteAsync(_buffer[..sizeof(double)], cancellationToken).ConfigureAwait(false);
    }
}
