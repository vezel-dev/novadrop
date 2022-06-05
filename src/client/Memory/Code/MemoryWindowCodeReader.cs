namespace Vezel.Novadrop.Memory.Code;

public sealed class MemoryWindowCodeReader : CodeReader
{
    public MemoryWindow CurrentWindow { get; private set; }

    public MemoryWindowCodeReader(MemoryWindow window)
    {
        _ = window.Accessor ?? throw new ArgumentException(null, nameof(window));

        CurrentWindow = window;
    }

    public override int ReadByte()
    {
        if (CurrentWindow.IsEmpty)
            return -1;

        var b = CurrentWindow.Read<byte>(0);

        CurrentWindow = CurrentWindow.Slice(1);

        return b;
    }
}
