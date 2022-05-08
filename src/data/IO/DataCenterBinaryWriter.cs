namespace Vezel.Novadrop.Data.IO;

sealed class DataCenterBinaryWriter
{
    readonly Stream _stream;

    readonly Memory<byte> _buffer = new byte[sizeof(uint)];

    public DataCenterBinaryWriter(Stream stream)
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
}
