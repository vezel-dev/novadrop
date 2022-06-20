namespace Vezel.Novadrop.IO;

sealed class StreamBinaryReader
{
    public long Progress { get; private set; }

    readonly Stream _stream;

    readonly Memory<byte> _buffer = GC.AllocateUninitializedArray<byte>(sizeof(ulong));

    public StreamBinaryReader(Stream stream)
    {
        _stream = stream;
    }

    public async ValueTask ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var progress = 0;

        while (progress < buffer.Length)
        {
            var len = await _stream.ReadAsync(buffer[progress..], cancellationToken).ConfigureAwait(false);

            if (len == 0)
                throw new EndOfStreamException();

            progress += len;
        }

        Progress += progress;
    }

    public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
    {
        await ReadAsync(_buffer[..sizeof(byte)], cancellationToken).ConfigureAwait(false);

        return _buffer.Span[0];
    }

    public async ValueTask<sbyte> ReadSByteAsync(CancellationToken cancellationToken)
    {
        return (sbyte)await ReadByteAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<ushort> ReadUInt16Async(CancellationToken cancellationToken)
    {
        await ReadAsync(_buffer[..sizeof(ushort)], cancellationToken).ConfigureAwait(false);

        return BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Span);
    }

    public async ValueTask<short> ReadInt16Async(CancellationToken cancellationToken)
    {
        return (short)await ReadUInt16Async(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<uint> ReadUInt32Async(CancellationToken cancellationToken)
    {
        await ReadAsync(_buffer[..sizeof(uint)], cancellationToken).ConfigureAwait(false);

        return BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Span);
    }

    public async ValueTask<int> ReadInt32Async(CancellationToken cancellationToken)
    {
        return (int)await ReadUInt32Async(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<ulong> ReadUInt64Async(CancellationToken cancellationToken)
    {
        await ReadAsync(_buffer[..sizeof(ulong)], cancellationToken).ConfigureAwait(false);

        return BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Span);
    }

    public async ValueTask<long> ReadInt64Async(CancellationToken cancellationToken)
    {
        return (long)await ReadUInt64Async(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<float> ReadSingleAsync(CancellationToken cancellationToken)
    {
        var value = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

        return Unsafe.As<uint, float>(ref value);
    }

    public async ValueTask<double> ReadDoubleAsync(CancellationToken cancellationToken)
    {
        var value = await ReadUInt64Async(cancellationToken).ConfigureAwait(false);

        return Unsafe.As<ulong, double>(ref value);
    }

    public async ValueTask<string> ReadStringAsync(CancellationToken cancellationToken)
    {
        var sb = new StringBuilder(1024);

        char c;

        while ((c = (char)await ReadUInt16Async(cancellationToken).ConfigureAwait(false)) != '\0')
            _ = sb.Append(c);

        return sb.ToString();
    }
}
