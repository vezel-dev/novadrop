namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x470)]
public unsafe struct S1Context
{
    [FieldOffset(0x8)]
    public S1CommandQueue* command_queue;

    [FieldOffset(0x18)]
    public S1ConnectionManager* connection_manager;
}
