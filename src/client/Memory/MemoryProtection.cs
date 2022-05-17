namespace Vezel.Novadrop.Memory;

[Flags]
public enum MemoryProtection
{
    None = 0b000,
    Read = 0b001,
    Write = 0b010,
    Execute = 0b100,
}
