using static Windows.Win32.WindowsPInvoke;

namespace Vezel.Novadrop.Memory.Code;

public sealed class DynamicCode : IDisposable
{
    public MemoryWindow Window { get; }

    public nuint Length { get; }

    DynamicCode(MemoryWindow window, nuint length)
    {
        Window = window;
        Length = length;
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
        Window.Process.Free(Window.Address);
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
        var ptr = process.Alloc(len, MemoryFlags.Read | MemoryFlags.Write);

        var window = new MemoryWindow(process, ptr, len);
        var windowWriter = new MemoryWindowCodeWriter(window);

        try
        {
            // Now assemble the code into the process for real.
            _ = asm.Assemble(windowWriter, ptr);

            // Fill the rest with interrupt instructions to catch mistakes.
            for (nuint i = 0; i < windowWriter.CurrentWindow.Length; i++)
                windowWriter.WriteByte(0xcc);

            process.Protect(ptr, len, MemoryFlags.Read | MemoryFlags.Execute);
            process.Flush(ptr, len);
        }
        catch (Exception)
        {
            process.Free(ptr);

            throw;
        }

        return new(window, len - windowWriter.CurrentWindow.Length);
    }

    public unsafe uint Call(nuint parameter)
    {
        using var handle = CreateRemoteThread(
            Window.Process.Handle,
            null,
            0,
            (delegate* unmanaged[Stdcall]<void*, uint>)Window.Address,
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
}
