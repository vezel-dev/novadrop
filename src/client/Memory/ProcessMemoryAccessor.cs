namespace Vezel.Novadrop.Memory;

public sealed class ProcessMemoryAccessor : MemoryAccessor
{
    public NativeProcess Process { get; }

    internal ProcessMemoryAccessor(NativeProcess process)
    {
        Process = process;
    }

    public override void Read(NativeAddress address, Span<byte> buffer)
    {
        Process.Read(address, buffer);
    }

    public override void Write(NativeAddress address, ReadOnlySpan<byte> buffer)
    {
        Process.Write(address, buffer);
    }
}
