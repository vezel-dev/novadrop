// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Buffers;

internal ref struct SpanWriter
{
    private Span<byte> _remaining;

    public SpanWriter(Span<byte> span)
    {
        _remaining = span;
    }

    public void Advance(int count)
    {
        _remaining = _remaining[count..];
    }

    public void Write(scoped ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(_remaining);

        Advance(buffer.Length);
    }

    public void WriteByte(byte value)
    {
        _remaining[0] = value;

        Advance(sizeof(byte));
    }

    public void WriteSByte(sbyte value)
    {
        WriteByte((byte)value);
    }

    public void WriteUInt16(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_remaining, value);

        Advance(sizeof(ushort));
    }

    public void WriteInt16(short value)
    {
        WriteUInt16((ushort)value);
    }

    public void WriteUInt32(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_remaining, value);

        Advance(sizeof(uint));
    }

    public void WriteInt32(int value)
    {
        WriteUInt32((uint)value);
    }

    public void WriteUInt64(ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(_remaining, value);

        Advance(sizeof(ulong));
    }

    public void WriteInt64(long value)
    {
        WriteUInt64((ulong)value);
    }

    public void WriteSingle(float value)
    {
        WriteUInt32(Unsafe.BitCast<float, uint>(value));
    }

    public void WriteDouble(double value)
    {
        WriteUInt64(Unsafe.BitCast<double, ulong>(value));
    }

    public void WriteChar(char value)
    {
        WriteUInt16(value);
    }
}
