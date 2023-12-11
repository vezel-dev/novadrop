using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe struct S1GameServerConnection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public IS1ConnectionInfo.VirtualFunctionTable IS1ConnectionInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable_IS1ConnectionEventHandler
    {
        public IS1ConnectionEventHandler.VirtualFunctionTable IS1ConnectionEventHandler;
    }

    public static delegate* unmanaged<S1GameServerConnection*, S1GameServerConnection*> __ctor { get; } =
        (delegate* unmanaged<S1GameServerConnection*, S1GameServerConnection*>)Tera.Resolve(0x7ff69bc05b20);

    public static delegate* unmanaged<S1GameServerConnection*, uint, S1GameServerConnection*> __vdtor { get; } =
        (delegate* unmanaged<S1GameServerConnection*, uint, S1GameServerConnection*>)Tera.Resolve(0x7ff69bc082b0);

    public static delegate* unmanaged<S1GameServerConnection*, S1ClientSocket*> CreateSocket { get; } =
        (delegate* unmanaged<S1GameServerConnection*, S1ClientSocket*>)Tera.Resolve(0x7ff69bc0ab30);

    public static delegate* unmanaged<S1GameServerConnection*, BOOL, void> OnConnect { get; } =
        (delegate* unmanaged<S1GameServerConnection*, BOOL, void>)Tera.Resolve(0x7ff69bc191c0);

    public static delegate* unmanaged<S1GameServerConnection*, void> OnDisconnect { get; } =
        (delegate* unmanaged<S1GameServerConnection*, void>)Tera.Resolve(0x7ff69bc18890);

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public IS1ConnectionInfo IS1ConnectionInfo;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;

    [FieldOffset(0x8)]
    public IS1ConnectionEventHandler IS1ConnectionEventHandler;

    [FieldOffset(0x8)]
    public VirtualFunctionTable_IS1ConnectionEventHandler VFT_IS1ConnectionEventHandler;
}
