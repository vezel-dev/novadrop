// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x9c)]
public unsafe struct S1ServerInfo
{
    [FieldOffset(0x0)]
    public int id;

    [FieldOffset(0x4)]
    public FString name;

    [FieldOffset(0x18)]
    public FString category;

    [FieldOffset(0x2c)]
    public FString title;

    [FieldOffset(0x40)]
    public FString queue;

    [FieldOffset(0x54)]
    public FString population;

    [FieldOffset(0x68)]
    public in_addr address;

    [FieldOffset(0x6c)]
    public ushort port;

    [FieldOffset(0x70)]
    public BOOL available;

    [FieldOffset(0x74)]
    public FString unavailable_message;

    [FieldOffset(0x88)]
    public FString host;
}
