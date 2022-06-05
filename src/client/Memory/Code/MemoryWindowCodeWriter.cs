namespace Vezel.Novadrop.Memory.Code;

public sealed class MemoryWindowCodeWriter : CodeWriter
{
    public MemoryWindow CurrentWindow { get; private set; }

    public MemoryWindowCodeWriter(MemoryWindow window)
    {
        _ = window.Accessor ?? throw new ArgumentException(null, nameof(window));

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
