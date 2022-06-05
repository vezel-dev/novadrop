namespace Vezel.Novadrop.Memory;

public sealed class ManagedMemoryAccessor : MemoryAccessor
{
    public Memory<byte> Memory { get; }

    public ManagedMemoryAccessor(Memory<byte> memory)
    {
        Memory = memory;
    }

    public override void Read(NativeAddress address, Span<byte> buffer)
    {
        Memory.Span.Slice((int)(nuint)address, buffer.Length).CopyTo(buffer);
    }

    public override void Write(NativeAddress address, ReadOnlySpan<byte> buffer)
    {
        buffer.CopyTo(Memory.Span[(int)(nuint)address..]);
    }
}
