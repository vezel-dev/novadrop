namespace Vezel.Novadrop.Memory;

public abstract class MemoryAccessor
{
    public abstract void Read(NativeAddress address, scoped Span<byte> buffer);

    public abstract void Write(NativeAddress address, scoped ReadOnlySpan<byte> buffer);
}
