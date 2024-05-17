// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0xa0)]
public unsafe struct FBufferReader
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public FArchive.VirtualFunctionTable FArchive;
    }

    public static delegate* unmanaged<FBufferReader*, void*, int, BOOL, BOOL, FBufferReader*> __ctor { get; } =
        (delegate* unmanaged<FBufferReader*, void*, int, BOOL, BOOL, FBufferReader*>)Tera.Resolve(0x7ff69a852b50);

    public static delegate* unmanaged<FBufferReader*, void> __dtor { get; } =
        (delegate* unmanaged<FBufferReader*, void>)Tera.Resolve(0x7ff69a854b20);

    public static delegate* unmanaged<FBufferReader*, FBufferReader*> __vdtor { get; } =
        (delegate* unmanaged<FBufferReader*, FBufferReader*>)Tera.Resolve(0x7ff69a8582e0);

    public static delegate* unmanaged<FBufferReader*, BOOL> AtEnd { get; } =
        (delegate* unmanaged<FBufferReader*, BOOL>)Tera.Resolve(0x7ff69a85be40);

    public static delegate* unmanaged<FBufferReader*, BOOL> Close { get; } =
        (delegate* unmanaged<FBufferReader*, BOOL>)Tera.Resolve(0x7ff69a85bfc0);

    public static delegate* unmanaged<FBufferReader*, int, void> Seek { get; } =
        (delegate* unmanaged<FBufferReader*, int, void>)Tera.Resolve(0x7ff69a879ef0);

    public static delegate* unmanaged<FBufferReader*, void*, int, void> Serialize { get; } =
        (delegate* unmanaged<FBufferReader*, void*, int, void>)Tera.Resolve(0x7ff69a87a370);

    public static delegate* unmanaged<FBufferReader*, int> Tell { get; } =
        (delegate* unmanaged<FBufferReader*, int>)Tera.Resolve(0x7ff69a87faf0);

    [FieldOffset(0x0)]
    public FArchive FArchive;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;

    [FieldOffset(0x8c)]
    public void* data;

    [FieldOffset(0x94)]
    public int position;

    [FieldOffset(0x98)]
    public int size;

    [FieldOffset(0x9c)]
    public BOOL free_on_close;
}
