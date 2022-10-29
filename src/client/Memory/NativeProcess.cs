using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.ToolHelp;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Memory;

public sealed unsafe class NativeProcess : IDisposable
{
    public int Id { get; }

    public SafeHandle Handle { get; }

    public ProcessMemoryAccessor Accessor { get; }

    public NativeModule MainModule => Modules.First();

    public IEnumerable<NativeModule> Modules
    {
        get
        {
            Check.Usable(!_disposed, this);

            return GetModules();
        }
    }

    private volatile bool _disposed;

    public NativeProcess(int id)
    {
        var handle = OpenProcess_SafeHandle(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)id);

        if (handle.IsInvalid)
            throw new Win32Exception();

        Id = id;
        Handle = handle;
        Accessor = new(this);
    }

    ~NativeProcess()
    {
        DisposeCore();
    }

    public void Dispose()
    {
        DisposeCore();

        GC.SuppressFinalize(this);
    }

    private static PAGE_PROTECTION_FLAGS TranslateProtection(MemoryProtection protection)
    {
        return protection switch
        {
            MemoryProtection.None =>
                PAGE_PROTECTION_FLAGS.PAGE_NOACCESS,
            MemoryProtection.Read =>
                PAGE_PROTECTION_FLAGS.PAGE_READONLY,
            MemoryProtection.Read | MemoryProtection.Write =>
                PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryProtection.Read | MemoryProtection.Execute =>
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READ,
            MemoryProtection.Read | MemoryProtection.Write | MemoryProtection.Execute =>
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryProtection.Write =>
                PAGE_PROTECTION_FLAGS.PAGE_READWRITE,
            MemoryProtection.Write | MemoryProtection.Execute =>
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE,
            MemoryProtection.Execute =>
                PAGE_PROTECTION_FLAGS.PAGE_EXECUTE,
            _ => throw new ArgumentOutOfRangeException(nameof(protection)),
        };
    }

    private IEnumerable<NativeModule> GetModules()
    {
        SafeFileHandle snap;

        while ((snap = CreateToolhelp32Snapshot_SafeHandle(
            CREATE_TOOLHELP_SNAPSHOT_FLAGS.TH32CS_SNAPMODULE, (uint)Id)).IsInvalid)
        {
            if (!snap.IsInvalid)
                break;

            // We may get ERROR_BAD_LENGTH for processes that have not finished initializing or if the process loads
            // or unloads a module while we are capturing the snapshot.
            if (Marshal.GetLastPInvokeError() != (int)WIN32_ERROR.ERROR_BAD_LENGTH)
                throw new Win32Exception();
        }

        using (snap)
        {
            var entry = new MODULEENTRY32W
            {
                dwSize = (uint)Unsafe.SizeOf<MODULEENTRY32W>(),
            };

            var result = Module32FirstW(snap, ref entry);

            while (true)
            {
                if (!result)
                {
                    if (Marshal.GetLastPInvokeError() != (int)WIN32_ERROR.ERROR_NO_MORE_FILES)
                        throw new Win32Exception();

                    break;
                }

                NativeModule CreateModule(in MODULEENTRY32W entry)
                {
                    // Cannot use unsafe code in iterators...
                    using var modHandle = new SafeFileHandle(entry.hModule, false);

                    var arr = new char[MAX_PATH];

                    uint len;

                    fixed (char* p = arr)
                        while ((len = K32GetModuleBaseNameW(Handle, modHandle, p, (uint)arr.Length)) >= arr.Length)
                            Array.Resize(ref arr, (int)len);

                    return len != 0
                        ? new(
                            arr.AsSpan(0, (int)len).ToString(),
                            new(Accessor, (NativeAddress)(nuint)entry.modBaseAddr, entry.modBaseSize))
                        : throw new Win32Exception();
                }

                yield return CreateModule(entry);

                result = Module32NextW(snap, ref entry);
            }
        }
    }

    private void DisposeCore()
    {
        _disposed = true;

        Handle.Dispose();
    }

    public NativeAddress Alloc(nuint length, MemoryProtection protection)
    {
        Check.Usable(!_disposed, this);

        return VirtualAllocEx(
            Handle,
            null,
            length,
            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
            TranslateProtection(protection)) is var ptr && ptr != null
            ? new((nuint)ptr)
            : throw new Win32Exception();
    }

    public void Free(NativeAddress address)
    {
        Check.Usable(!_disposed, this);

        if (!VirtualFreeEx(Handle, (void*)(nuint)address, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE))
            throw new Win32Exception();
    }

    public void Protect(NativeAddress address, nuint length, MemoryProtection protection)
    {
        Check.Usable(!_disposed, this);

        if (!VirtualProtectEx(Handle, (void*)(nuint)address, length, TranslateProtection(protection), out _))
            throw new Win32Exception();
    }

    public void Read(NativeAddress address, scoped Span<byte> buffer)
    {
        Check.Usable(!_disposed, this);

        fixed (byte* p = buffer)
            if (!ReadProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public void Write(NativeAddress address, scoped ReadOnlySpan<byte> buffer)
    {
        Check.Usable(!_disposed, this);

        fixed (byte* p = buffer)
            if (!WriteProcessMemory(Handle, (void*)(nuint)address, p, (nuint)buffer.Length, null))
                throw new Win32Exception();
    }

    public void Flush(NativeAddress address, nuint length)
    {
        Check.Usable(!_disposed, this);

        if (!FlushInstructionCache(Handle, (void*)(nuint)address, length))
            throw new Win32Exception();
    }

    private void ForEachThread(Func<int, bool> predicate, Action<uint, SafeFileHandle> action)
    {
        Check.Null(predicate);
        Check.Usable(!_disposed, this);

        var pid = (uint)Id;

        using var snap = CreateToolhelp32Snapshot_SafeHandle(CREATE_TOOLHELP_SNAPSHOT_FLAGS.TH32CS_SNAPTHREAD, pid);

        if (snap.IsInvalid)
            throw new Win32Exception();

        var entry = new THREADENTRY32
        {
            dwSize = (uint)sizeof(THREADENTRY32),
        };

        var result = Thread32First(snap, ref entry);

        while (true)
        {
            if (!result)
            {
                if (Marshal.GetLastPInvokeError() != (int)WIN32_ERROR.ERROR_NO_MORE_FILES)
                    throw new Win32Exception();

                break;
            }

            if (entry.dwSize != sizeof(THREADENTRY32) ||
                entry.th32OwnerProcessID != pid ||
                !predicate((int)entry.th32ThreadID))
                continue;

            using var handle = OpenThread_SafeHandle(THREAD_ACCESS_RIGHTS.THREAD_ALL_ACCESS, false, entry.th32ThreadID);

            if (!handle.IsInvalid)
                action(entry.th32ThreadID, handle);

            result = Thread32Next(snap, ref entry);
        }
    }

    public (int Id, int Count)[] Suspend(Func<int, bool> predicate)
    {
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
        return $"{{Id: {Id}, Name: {MainModule.Name}}}";
    }
}
