// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct ILauncherEvent
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public delegate* unmanaged<ILauncherEvent*, BOOL, S1ServerList*, void> OnServerList;

        public delegate* unmanaged<ILauncherEvent*, byte*, void> OnCreateRoom;

        public delegate* unmanaged<ILauncherEvent*, BOOL, void> OnJoinRoom;

        public delegate* unmanaged<ILauncherEvent*, BOOL, void> OnLeaveRoom;
    }

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
