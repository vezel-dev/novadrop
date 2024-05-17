// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct S1ServerDataEvent
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public ILauncherEvent.VirtualFunctionTable ILauncherEvent;
    }

    public static delegate* unmanaged<S1ServerDataEvent*, BOOL, S1ServerList*, void> OnServerList { get; } =
        (delegate* unmanaged<S1ServerDataEvent*, BOOL, S1ServerList*, void>)Tera.Resolve(0x7ff69bd24dd0);

    [FieldOffset(0x0)]
    public ILauncherEvent ILauncherEvent;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
