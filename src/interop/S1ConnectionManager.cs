// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x50)]
public unsafe struct S1ConnectionManager
{
    public static delegate* unmanaged<byte*, int, int, uint> ComputeChecksum { get; } =
        (delegate* unmanaged<byte*, int, int, uint>)Tera.Resolve(0x7ff69b472da0);

    public static delegate* unmanaged<S1ConnectionManager*, void> Dispose { get; } =
        (delegate* unmanaged<S1ConnectionManager*, void>)Tera.Resolve(0x7ff69babc8a0);

    public static delegate* unmanaged<S1ConnectionManager*, FName*, S1Connection*> GetConnection { get; } =
        (delegate* unmanaged<S1ConnectionManager*, FName*, S1Connection*>)Tera.Resolve(0x7ff69baafcc0);

    public static delegate* unmanaged<FName*, FName*> GetAccountServerName { get; } =
        (delegate* unmanaged<FName*, FName*>)Tera.Resolve(0x7ff69b9b47e0);

    public static delegate* unmanaged<FName*, FName*> GetGameServerName { get; } =
        (delegate* unmanaged<FName*, FName*>)Tera.Resolve(0x7ff69bc12b60);

    public static delegate* unmanaged<S1ConnectionManager*, byte*, void> SendGamePacket { get; } =
        (delegate* unmanaged<S1ConnectionManager*, byte*, void>)Tera.Resolve(0x7ff69babe750);

    public static delegate* unmanaged<S1ConnectionManager*, FName*, byte*, uint, void> SendPacket { get; } =
        (delegate* unmanaged<S1ConnectionManager*, FName*, byte*, uint, void>)Tera.Resolve(0x7ff69babe7b0);

    [FieldOffset(0x4c)]
    public BOOL exiting;
}
