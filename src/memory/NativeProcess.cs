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

    readonly object _lock = new();

    public NativeProcess(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        // We need to open a second handle with full permissions.
        Process = process;
        Handle = OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)process.Id);

        if (Handle.IsInvalid)
            throw new Win32Exception();
    }

    static PAGE_PROTECTION_FLAGS TranslateFlags(MemoryFlags flags)
    {
        return flags switch
        {
            MemoryFlags.None => PAGE_PROTECTION_FLAGS.PAGE_NOACCESS,
            MemoryFlags.Read => PAGE_PROTECTION_FLAGS.PAGE_READONLY,
            MemoryFlags.Read | MemoryFlags.Write => PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryFlags.Read | MemoryFlags.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READ,
            MemoryFlags.Read | MemoryFlags.Write | MemoryFlags.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryFlags.Write => PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryFlags.Write | MemoryFlags.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryFlags.Execute => PAGE_PROTECTION_FLAGS.PAGE_EXECUTE,
            _ => throw new ArgumentOutOfRangeException(nameof(flags)),
        };
    }

    MemoryWindow WrapModule(ProcessModule module)
    {
        return new(this, (nuint)(nint)module.BaseAddress, (nuint)module.ModuleMemorySize);
    }

    public unsafe nuint Alloc(nuint length, MemoryFlags flags)
    {
        return VirtualAllocEx(
            Handle,
            null,
            length,
            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
            TranslateFlags(flags)) is var ptr && ptr != null
            ? (nuint)ptr
            : throw new Win32Exception();
    }

    public unsafe void Free(nuint address)
    {
        if (!VirtualFreeEx(Handle, (void*)address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE))
            throw new Win32Exception();
    }

    public unsafe void Protect(nuint address, nuint length, MemoryFlags flags)
    {
        if (!VirtualProtectEx(Handle, (void*)address, length, TranslateFlags(flags), out _))
            throw new Win32Exception();
    }

    public unsafe void Read(nuint address, Span<byte> buffer)
    {
        lock (_lock)
            fixed (byte* p = buffer)
                if (!ReadProcessMemory(Handle, (void*)address, p, (nuint)buffer.Length, null))
                    throw new Win32Exception();
    }

    public unsafe void Write(nuint address, ReadOnlySpan<byte> buffer)
    {
        lock (_lock)
            fixed (byte* p = buffer)
                if (!WriteProcessMemory(Handle, (void*)address, p, (nuint)buffer.Length, null))
                    throw new Win32Exception();
    }

    public unsafe void Flush(nuint address, nuint length)
    {
        if (!FlushInstructionCache(Handle, (void*)address, length))
            throw new Win32Exception();
    }
}
