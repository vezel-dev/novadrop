namespace Vezel.Novadrop.Memory;

public sealed class ProcessMemoryAccessor : MemoryAccessor
{
    public NativeProcess Process { get; }

    internal ProcessMemoryAccessor(NativeProcess process)
    {
        Process = process;
    }

    public override void Read(NativeAddress address, scoped Span<byte> buffer)
    {
        Process.Read(address, buffer);
    }

    public override void Write(NativeAddress address, scoped ReadOnlySpan<byte> buffer)
    {
        Process.Write(address, buffer);
    }
}
