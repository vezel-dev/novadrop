namespace Vezel.Novadrop.Memory;

[Flags]
[SuppressMessage("", "CA1711")]
public enum MemoryFlags
{
    None = 0b000,
    Read = 0b001,
    Write = 0b010,
    Execute = 0b100,
}
