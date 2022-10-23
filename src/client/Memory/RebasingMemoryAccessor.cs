namespace Vezel.Novadrop.Memory;

public sealed class RebasingMemoryAccessor : MemoryAccessor
{
    public MemoryAccessor Accessor { get; }

    public NativeAddress Address { get; }

    public RebasingMemoryAccessor(MemoryAccessor accessor, NativeAddress address)
    {
        Check.Null(accessor);

        Accessor = accessor;
        Address = address;
    }

    public override void Read(NativeAddress address, scoped Span<byte> buffer)
    {
        Accessor.Read(address - Address, buffer);
    }

    public override void Write(NativeAddress address, scoped ReadOnlySpan<byte> buffer)
    {
        Accessor.Write(address - Address, buffer);
    }
}
