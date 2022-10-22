namespace Vezel.Novadrop.Memory.Code;

public sealed class MemoryWindowCodeWriter : CodeWriter
{
    public MemoryWindow CurrentWindow { get; private set; }

    public MemoryWindowCodeWriter(MemoryWindow window)
    {
        Check.Argument(window.Accessor != null, window);

        CurrentWindow = window;
    }

    public override void WriteByte(byte value)
    {
        // This is what MemoryStream/UnmanagedMemoryStream throw in this case.
        if (CurrentWindow.IsEmpty)
            throw new NotSupportedException();

        CurrentWindow.Write(0, value);
        CurrentWindow = CurrentWindow.Slice(1);
    }
}
