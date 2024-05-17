// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe struct S1Connection
{
    public static delegate* unmanaged<S1Connection*, uint, ushort, BOOL> Connect { get; } =
        (delegate* unmanaged<S1Connection*, uint, ushort, BOOL>)Tera.Resolve(0x7ff69baa9fc0);

    public static delegate* unmanaged<S1Connection*, void> Disconnect { get; } =
        (delegate* unmanaged<S1Connection*, void>)Tera.Resolve(0x7ff69baaa530);

    [FieldOffset(0x0)]
    public IS1ConnectionInfo* info;

    [FieldOffset(0x8)]
    public S1ClientSocket* socket;
}
