using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct IS1ConnectionEventHandler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public delegate* unmanaged<IS1ConnectionEventHandler*, BOOL, void> OnConnect;

        public delegate* unmanaged<IS1ConnectionEventHandler*, void> OnDisconnect;

        public delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void> OnReceive;
    }

    public static delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void> OnReceive { get; } =
        (delegate* unmanaged<IS1ConnectionEventHandler*, byte*, int, void>)Tera.Resolve(0x7ff69b9c1ce0);

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
