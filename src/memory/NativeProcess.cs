using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Memory;

public sealed class NativeProcess
{
    public Process Process { get; }

    public SafeHandle Handle { get; }

    public MemoryWindow MainModule => WrapModule(Process.MainModule!);

    public IEnumerable<MemoryWindow> Modules => Process.Modules.Cast<ProcessModule>().Select(WrapModule);

    public NativeProcess(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        // We need to open a second handle with full permissions.
        Process = process;
        Handle = OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)process.Id);

        if (Handle.IsInvalid)
            throw new Win32Exception();
    }

    static PAGE_PROTECTION_FLAGS TranslateProtection(MemoryProtection protection)
    {
        return protection switch
        {
            MemoryProtection.None => PAGE_PROTECTION_FLAGS.PAGE_NOACCESS,
            MemoryProtection.Read => PAGE_PROTECTION_FLAGS.PAGE_READONLY,
            MemoryProtection.Read | MemoryProtection.Write => PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryProtection.Read | MemoryProtection.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READ,
            MemoryProtection.Read | MemoryProtection.Write | MemoryProtection.Execute =>
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryProtection.Write => PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryProtection.Write | MemoryProtection.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryProtection.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE,
            _ => throw new ArgumentOutOfRangeException(nameof(protection)),
        };
    }

    MemoryWindow WrapModule(ProcessModule module)
    {
        return new(this, (NativeAddress)(nuint)(nint)module.BaseAddress, (nuint)module.ModuleMemorySize);
    }

    public unsafe NativeAddress Alloc(nuint length, MemoryProtection protection)
    {
        return VirtualAllocEx(
            Handle,
            null,
            length,
            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
            TranslateProtection(protection)) is var ptr && ptr != null
            ? new((nuint)ptr)
            : throw new Win32Exception();
    }

    public unsafe void Free(NativeAddress address)
    {
        if (!VirtualFreeEx(Handle, (void*)(nuint)address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE))
            throw new Win32Exception();
    }

    public unsafe void Protect(NativeAddress address, nuint length, MemoryProtection protection)
    {
        if (!VirtualProtectEx(Handle, (void*)(nuint)address, length, TranslateProtection(protection), out _))
            throw new Win32Exception();
    }

    public unsafe void Read(NativeAddress address, Span<byte> buffer)
    {
        fixed (byte* p = buffer)
            if (!ReadProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public unsafe void Write(NativeAddress address, ReadOnlySpan<byte> buffer)
    {
        fixed (byte* p = buffer)
            if (!WriteProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public unsafe void Flush(NativeAddress address, nuint length)
    {
        if (!FlushInstructionCache(Handle, (void*)(nuint)address, length))
            throw new Win32Exception();
    }

    public override string ToString()
    {
        return $"{{Id: {Process.Id}, Name: {Process.ProcessName}}}";
    }
}
