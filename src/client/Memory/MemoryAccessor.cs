namespace Vezel.Novadrop.Memory;

public abstract class MemoryAccessor
{
    public abstract void Read(NativeAddress address, Span<byte> buffer);

    public abstract void Write(NativeAddress address, ReadOnlySpan<byte> buffer);
}
