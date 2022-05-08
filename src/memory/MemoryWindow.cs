namespace Vezel.Novadrop.Memory;

public readonly struct MemoryWindow : IEquatable<MemoryWindow>
{
    public NativeProcess Process { get; }

    public nuint Address { get; }

    public nuint Length { get; }

    public bool IsEmpty => Length == 0;

    public MemoryWindow(NativeProcess process, nuint address, nuint length)
    {
        ArgumentNullException.ThrowIfNull(process);

        Process = process;
        Address = address;
        Length = length;
    }

    public static bool operator ==(MemoryWindow left, MemoryWindow right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MemoryWindow left, MemoryWindow right)
    {
        return !left.Equals(right);
    }

    public bool ContainsAddress(nuint address)
    {
        return address >= Address && address < Address + Length;
    }

    public bool ContainsOffset(nuint offset)
    {
        return offset >= 0 && offset < Length;
    }

    public bool ContainsRange(nuint offset, nuint length)
    {
        return offset <= Length && length <= Length - offset;
    }

    public nuint ToAddress(nuint offset)
    {
        _ = ContainsOffset(offset) ? true : throw new ArgumentOutOfRangeException(nameof(offset));

        return Address + offset;
    }

    public unsafe nuint ToOffset(nuint address)
    {
        _ = ContainsAddress(address) ? true : throw new ArgumentOutOfRangeException(nameof(address));

        return address - Address;
    }

    public bool TryGetAddress(nuint offset, out nuint address)
    {
        try
        {
            address = ToAddress(offset);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            address = default;
            return false;
        }
    }

    public bool TryGetOffset(nuint address, out nuint offset)
    {
        try
        {
            offset = ToOffset(address);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            offset = default;
            return false;
        }
    }

    public MemoryWindow Slice(nuint offset)
    {
        return Slice(offset, Length - offset);
    }

    public MemoryWindow Slice(nuint offset, nuint length)
    {
        _ = ContainsRange(offset, length) ? true : throw new ArgumentOutOfRangeException(nameof(offset));

        return new(Process, Address + offset, length);
    }

    public IEnumerable<nuint> Search(ReadOnlyMemory<byte?> pattern)
    {
        // TODO: Optimize this mess.

        for (nuint offset = 0; ContainsRange(offset, (nuint)pattern.Length); offset++)
        {
            var span = pattern.Span;
            var match = true;

            for (var i = 0; i < span.Length; i++)
            {
                var b = span[i];

                if (b != null && Read<byte>(offset + (nuint)i) != b)
                {
                    match = false;
                    break;
                }
            }

            if (match)
                yield return offset;
        }
    }

    public void Read(nuint offset, Span<byte> buffer)
    {
        _ = ContainsRange(offset, (nuint)buffer.Length) ? true : throw new ArgumentOutOfRangeException(nameof(offset));

        Process.Read(ToAddress(offset), buffer);
    }

    public unsafe T Read<T>(nuint offset)
        where T : unmanaged
    {
        Unsafe.SkipInit(out T result);
        Read(offset, new(&result, sizeof(T)));

        return result;
    }

    public void Write(nuint offset, ReadOnlySpan<byte> buffer)
    {
        _ = ContainsRange(offset, (nuint)buffer.Length) ? true : throw new ArgumentOutOfRangeException(nameof(offset));

        Process.Write(ToAddress(offset), buffer);
    }

    public unsafe void Write<T>(nuint offset, T value)
        where T : unmanaged
    {
        Write(offset, new(&value, sizeof(T)));
    }

    public bool Equals(MemoryWindow other)
    {
        return Process == other.Process && Address == other.Address && Length == other.Length;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is MemoryWindow w && Equals(w);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Process, Address, Length);
    }

    public override string ToString()
    {
        return $"{{Process: {Process}, Address: 0x{Address:x}, Length: {Length}}}";
    }
}
