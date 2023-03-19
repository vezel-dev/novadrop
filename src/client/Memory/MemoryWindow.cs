namespace Vezel.Novadrop.Memory;

public readonly struct MemoryWindow : IEquatable<MemoryWindow>, IEqualityOperators<MemoryWindow, MemoryWindow, bool>
{
    public MemoryAccessor Accessor { get; }

    public NativeAddress Address { get; }

    public nuint Length { get; }

    public bool IsEmpty => Length == 0;

    public MemoryWindow(MemoryAccessor accessor, NativeAddress address, nuint length)
    {
        Check.Null(accessor);

        Accessor = accessor;
        Address = address;
        Length = length;
    }

    public static bool operator ==(MemoryWindow left, MemoryWindow right) => left.Equals(right);

    public static bool operator !=(MemoryWindow left, MemoryWindow right) => !left.Equals(right);

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

    public bool TryGetAddress(nuint offset, out NativeAddress address)
    {
        if (ContainsOffset(offset))
        {
            address = Address + offset;

            return true;
        }

        address = default;

        return false;
    }

    public bool TryGetOffset(NativeAddress address, out nuint offset)
    {
        if (ContainsAddress(address))
        {
            offset = (nuint)(address - Address);

            return true;
        }

        offset = 0;

        return false;
    }

    public NativeAddress ToAddress(nuint offset)
    {
        Check.Range(TryGetAddress(offset, out var addr), offset);

        return addr;
    }

    public nuint ToOffset(NativeAddress address)
    {
        Check.Range(TryGetOffset(address, out var off), address);

        return off;
    }

    public MemoryWindow Slice(nuint offset)
    {
        return Slice(offset, Length - offset);
    }

    public MemoryWindow Slice(nuint offset, nuint length)
    {
        Check.Range(ContainsRange(offset, length), offset);

        return new(Accessor, Address + offset, length);
    }

    public Task<IEnumerable<nuint>> SearchAsync(
        ReadOnlyMemory<byte?> pattern, CancellationToken cancellationToken = default)
    {
        return SearchAsync(pattern, -1, cancellationToken);
    }

    public Task<IEnumerable<nuint>> SearchAsync(
        ReadOnlyMemory<byte?> pattern, int maxDegreeOfParallelism, CancellationToken cancellationToken = default)
    {
        Check.Argument(!pattern.IsEmpty, pattern);
        Check.Range(maxDegreeOfParallelism is -1 or > 0, maxDegreeOfParallelism);

        var window = this;

        return SearchAsync();

        async Task<IEnumerable<nuint>> SearchAsync()
        {
            var length = pattern.Length;

            IEnumerable<long> EnumerateOffsets()
            {
                for (nuint i = 0; window.ContainsRange(i, (nuint)length); i++)
                    yield return (long)i;
            }

            var offsets = new List<nuint>();

            await Parallel
                .ForEachAsync(
                    EnumerateOffsets(),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = maxDegreeOfParallelism,
                        CancellationToken = cancellationToken,
                    },
                    (i, ct) =>
                    {
                        ct.ThrowIfCancellationRequested();

                        var offset = (nuint)i;
                        var candidateSpan = length <= 256 ? stackalloc byte[length] : new byte[length];

                        if (!window.TryRead(offset, candidateSpan))
                            return default;

                        var patternSpan = pattern.Span;
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

                        return default;
                    })
                .ConfigureAwait(false);

            return offsets;
        }
    }

    public bool TryRead(nuint offset, Span<byte> buffer)
    {
        if (!ContainsRange(offset, (nuint)buffer.Length))
            return false;

        Accessor.Read(ToAddress(offset), buffer);

        return true;
    }

    public bool TryRead<T>(nuint offset, out T value)
        where T : unmanaged
    {
        value = default;

        return TryRead(offset, MemoryMarshal.AsBytes(new Span<T>(ref value)));
    }

    public void Read(nuint offset, Span<byte> buffer)
    {
        Check.Range(TryRead(offset, buffer), offset);
    }

    public T Read<T>(nuint offset)
        where T : unmanaged
    {
        Check.Range(TryRead<T>(offset, out var value), offset);

        return value;
    }

    public bool TryWrite(nuint offset, scoped ReadOnlySpan<byte> buffer)
    {
        if (!ContainsRange(offset, (nuint)buffer.Length))
            return false;

        Accessor.Write(ToAddress(offset), buffer);

        return true;
    }

    public bool TryWrite<T>(nuint offset, T value)
        where T : unmanaged
    {
        return TryWrite(offset, MemoryMarshal.AsBytes(new ReadOnlySpan<T>(value)));
    }

    public void Write(nuint offset, scoped ReadOnlySpan<byte> buffer)
    {
        Check.Range(TryWrite(offset, buffer), offset);
    }

    public void Write<T>(nuint offset, T value)
        where T : unmanaged
    {
        Check.Range(TryWrite(offset, value), offset);
    }

    public bool Equals(MemoryWindow other)
    {
        return Accessor == other.Accessor && Address == other.Address && Length == other.Length;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is MemoryWindow w && Equals(w);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Accessor, Address, Length);
    }

    public override string ToString()
    {
        return $"{{Address: 0x{Address:x}, Length: {Length}}}";
    }
}
