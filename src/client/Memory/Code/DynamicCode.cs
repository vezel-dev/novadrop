using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Memory.Code;

public sealed class DynamicCode : IDisposable
{
    public NativeProcess Process { get; }

    public MemoryWindow FullWindow { get; }

    public MemoryWindow CodeWindow { get; }

    int _disposed;

    DynamicCode(NativeProcess process, MemoryWindow fullWindow, MemoryWindow codeWindow)
    {
        Process = process;
        FullWindow = fullWindow;
        CodeWindow = codeWindow;
    }

    ~DynamicCode()
    {
        Free();
    }

    public void Dispose()
    {
        Free();

        GC.SuppressFinalize(this);
    }

    void Free()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            Process.Free(FullWindow.Address);
    }

    public static unsafe DynamicCode Create(NativeProcess process, Action<Assembler> assembler)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(assembler);

        var asm = new Assembler(sizeof(nuint) * 8);

        assembler(asm);

        using var stream = new MemoryStream();
        var streamWriter = new StreamCodeWriter(stream);

        // Perform a first pass of assembly just to get an idea of what the code length will be.
        _ = asm.Assemble(streamWriter, 0);

        // Allocate space for the code. This is a huge overestimation but should always be correct.
        var len = (nuint)stream.Length * 2;
        var ptr = process.Alloc(len, MemoryProtection.Read | MemoryProtection.Write);

        var window = new MemoryWindow(process.Accessor, ptr, len);
        var windowWriter = new MemoryWindowCodeWriter(window);

        try
        {
            // Now assemble the code into the process for real.
            _ = asm.Assemble(windowWriter, (nuint)ptr);

            // Fill the rest with interrupt instructions to catch mistakes.
            for (nuint i = 0; i < windowWriter.CurrentWindow.Length; i++)
                windowWriter.WriteByte(0xcc);

            process.Protect(ptr, len, MemoryProtection.Read | MemoryProtection.Execute);
            process.Flush(ptr, len);
        }
        catch (Exception)
        {
            process.Free(ptr);

            throw;
        }

        return new(process, window, window.Slice(0, len - windowWriter.CurrentWindow.Length));
    }

    public unsafe uint Call(nuint parameter)
    {
        _ = _disposed == 0 ? true : throw new ObjectDisposedException(GetType().Name);

        using var handle = CreateRemoteThread(
            Process.Handle,
            null,
            0,
            (delegate* unmanaged[Stdcall]<void*, uint>)(nuint)FullWindow.Address,
            (void*)parameter,
            0,
            null);

        return !handle.IsInvalid
            ? WaitForSingleObject(handle, INFINITE) == WAIT_OBJECT_0
                ? GetExitCodeThread(handle, out var result)
                    ? result
                    : throw new Win32Exception()
                : throw new Win32Exception()
            : throw new Win32Exception();
    }

    public override string ToString()
    {
        return $"{{FullWindow: {FullWindow}, CodeWindow: {CodeWindow}}}";
    }
}
