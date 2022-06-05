using Windows.Win32.System.Diagnostics.ToolHelp;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Memory;

public sealed class NativeProcess : IDisposable
{
    public Process Process { get; }

    public SafeHandle Handle { get; }

    public MemoryWindow MainModule => WrapModule(Process.MainModule!);

    public IEnumerable<MemoryWindow> Modules => Process.Modules.Cast<ProcessModule>().Select(WrapModule);

    int _disposed;

    public NativeProcess(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);

        // We need to open a second handle with full permissions.
        Process = process;
        Handle = OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)process.Id);

        if (Handle.IsInvalid)
            throw new Win32Exception();
    }

    ~NativeProcess()
    {
        Free();
    }

    public void Dispose()
    {
        Free();

        GC.SuppressFinalize(this);
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

    void Free()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            Handle.Dispose();
    }

    MemoryWindow WrapModule(ProcessModule module)
    {
        return new(this, (NativeAddress)(nuint)(nint)module.BaseAddress, (nuint)module.ModuleMemorySize);
    }

    public unsafe NativeAddress Alloc(nuint length, MemoryProtection protection)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

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
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        if (!VirtualFreeEx(Handle, (void*)(nuint)address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE))
            throw new Win32Exception();
    }

    public unsafe void Protect(NativeAddress address, nuint length, MemoryProtection protection)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        if (!VirtualProtectEx(Handle, (void*)(nuint)address, length, TranslateProtection(protection), out _))
            throw new Win32Exception();
    }

    public unsafe void Read(NativeAddress address, Span<byte> buffer)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        fixed (byte* p = buffer)
            if (!ReadProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public unsafe void Write(NativeAddress address, ReadOnlySpan<byte> buffer)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        fixed (byte* p = buffer)
            if (!WriteProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public unsafe void Flush(NativeAddress address, nuint length)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        if (!FlushInstructionCache(Handle, (void*)(nuint)address, length))
            throw new Win32Exception();
    }

    unsafe void ForEachThread(Func<int, bool> predicate, Action<uint, SafeFileHandle> action)
    {
        var pid = (uint)Process.Id;
        using var snap = CreateToolhelp32Snapshot_SafeHandle(CREATE_TOOLHELP_SNAPSHOT_FLAGS.TH32CS_SNAPTHREAD, pid);

        if (snap.IsInvalid)
            throw new Win32Exception();

        var te = new THREADENTRY32
        {
            dwSize = (uint)sizeof(THREADENTRY32),
        };

        if (!Thread32First(snap, ref te))
            return;

        do
        {
            if (te.dwSize == sizeof(THREADENTRY32) && te.th32OwnerProcessID == pid && predicate((int)te.th32ThreadID))
            {
                using var handle = OpenThread_SafeHandle(
                    THREAD_ACCESS_RIGHTS.THREAD_SUSPEND_RESUME, false, te.th32ThreadID);

                if (!handle.IsInvalid)
                    action(te.th32ThreadID, handle);
            }
        }
        while (Thread32Next(snap, ref te));
    }

    public (int Id, int Count)[] Suspend(Func<int, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var suspended = new List<(int, int)>();

        ForEachThread(predicate, (tid, handle) =>
        {
            if (SuspendThread(handle) is not uint.MaxValue and var count)
                suspended.Add(((int)tid, (int)count));
        });

        return suspended.ToArray();
    }

    public (int Id, int Count)[] Resume(Func<int, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var resumed = new List<(int, int)>();

        ForEachThread(predicate, (tid, handle) =>
        {
            if (ResumeThread(handle) is not uint.MaxValue and var count)
                resumed.Add(((int)tid, (int)count));
        });

        return resumed.ToArray();
    }

    public override string ToString()
    {
        return $"{{Id: {Process.Id}, Name: {Process.ProcessName}}}";
    }
}
