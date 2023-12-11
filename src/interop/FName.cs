namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct FName
{
    [FieldOffset(0x0)]
    public int name_index;

    [FieldOffset(0x4)]
    public int number;
}
