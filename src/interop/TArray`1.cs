namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe struct TArray<T>
    where T : unmanaged
{
    [FieldOffset(0x0)]
    public T* elements;

    [FieldOffset(0x8)]
    public int count;

    [FieldOffset(0xc)]
    public int capacity;
}
