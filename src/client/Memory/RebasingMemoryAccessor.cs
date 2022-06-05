namespace Vezel.Novadrop.Memory;

public sealed class RebasingMemoryAccessor : MemoryAccessor
{
    public MemoryAccessor Accessor { get; }

    public NativeAddress Address { get; }

    public RebasingMemoryAccessor(MemoryAccessor accessor, NativeAddress address)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        Accessor = accessor;
        Address = address;
    }

    public override void Read(NativeAddress address, Span<byte> buffer)
    {
        Accessor.Read(address - Address, buffer);
    }

    public override void Write(NativeAddress address, ReadOnlySpan<byte> buffer)
    {
        Accessor.Write(address - Address, buffer);
    }
}
