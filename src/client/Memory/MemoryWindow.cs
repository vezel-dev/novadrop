namespace Vezel.Novadrop.Memory;

public readonly struct MemoryWindow : IEquatable<MemoryWindow>
{
    public NativeProcess Process { get; }

    public NativeAddress Address { get; }

    public nuint Length { get; }

    public bool IsEmpty => Length == 0;

    public MemoryWindow(NativeProcess process, NativeAddress address, nuint length)
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

    public bool ContainsAddress(NativeAddress address)
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

    public NativeAddress ToAddress(nuint offset)
    {
        _ = ContainsOffset(offset) ? true : throw new ArgumentOutOfRangeException(nameof(offset));

        return Address + offset;
    }

    public unsafe nuint ToOffset(NativeAddress address)
    {
        _ = ContainsAddress(address) ? true : throw new ArgumentOutOfRangeException(nameof(address));

        return (nuint)(address - Address);
    }

    public bool TryGetAddress(nuint offset, out NativeAddress address)
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

    public bool TryGetOffset(NativeAddress address, out nuint offset)
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
        var window = this;
        var offsets = new List<nuint>();

        _ = Parallel.For(0, (long)Length, (i, state) =>
        {
            var offset = (nuint)i;
            var length = pattern.Length;

            if (!window.ContainsRange(offset, (nuint)length))
            {
                state.Break();

                return;
            }

            var patternSpan = pattern.Span;
            var candidateSpan = length <= 256 ? stackalloc byte[length] : new byte[length];

            if (!window.TryRead(offset, candidateSpan))
            {
                state.Break();

                return;
            }

            var match = true;

            for (var j = 0; j < length; j++)
            {
                var b = Unsafe.Add(ref MemoryMarshal.GetReference(patternSpan), j);

                if (b != null && Unsafe.Add(ref MemoryMarshal.GetReference(candidateSpan), j) != b)
                {
                    match = false;

                    break;
                }
            }

            if (match)
                lock (offsets)
                    offsets.Add(offset);
        });

        return offsets;
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
