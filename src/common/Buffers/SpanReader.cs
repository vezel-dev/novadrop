// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Buffers;

internal ref struct SpanReader
{
    private ReadOnlySpan<byte> _remaining;

    public SpanReader(ReadOnlySpan<byte> span)
    {
        _remaining = span;
    }

    public void Advance(int count)
    {
        _remaining = _remaining[count..];
    }

    public void Read(scoped Span<byte> buffer)
    {
        _remaining.CopyTo(buffer);

        Advance(buffer.Length);
    }

    public byte ReadByte()
    {
        var result = _remaining[0];

        Advance(sizeof(byte));

        return result;
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public ushort ReadUInt16()
    {
        var result = BinaryPrimitives.ReadUInt16LittleEndian(_remaining);

        Advance(sizeof(ushort));

        return result;
    }

    public short ReadInt16()
    {
        return (short)ReadUInt16();
    }

    public uint ReadUInt32()
    {
        var result = BinaryPrimitives.ReadUInt32LittleEndian(_remaining);

        Advance(sizeof(uint));

        return result;
    }

    public int ReadInt32()
    {
        return (int)ReadUInt32();
    }

    public ulong ReadUInt64()
    {
        var result = BinaryPrimitives.ReadUInt64LittleEndian(_remaining);

        Advance(sizeof(ulong));

        return result;
    }

    public long ReadInt64()
    {
        return (long)ReadUInt64();
    }

    public float ReadSingle()
    {
        return Unsafe.BitCast<uint, float>(ReadUInt32());
    }

    public double ReadDouble()
    {
        return Unsafe.BitCast<ulong, double>(ReadUInt64());
    }

    public char ReadChar()
    {
        return (char)ReadUInt16();
    }
}
