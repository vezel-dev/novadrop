// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8c)]
public unsafe struct FArchive
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public delegate* unmanaged<FArchive*, uint, FArchive*> __vdtor;

        public delegate* unmanaged<FArchive*, void*, int> Serialize;

        public delegate* unmanaged<FArchive*, void*, int> SerializeBits;

        public delegate* unmanaged<FArchive*, uint*, uint> SerializeInt;

        public void* __slot4;

        public delegate* unmanaged<FArchive*, ulong, ulong> CountBytes;

        public void* __slot6;

        public void* __slot7;

        public void* __slot8;

        public void* __slot9;

        public delegate* unmanaged<FArchive*, int> Tell;

        public delegate* unmanaged<FArchive*, int> TotalSize;

        public delegate* unmanaged<FArchive*, BOOL> AtEnd;

        public delegate* unmanaged<FArchive*, int, void> Seek;

        public void* __slot14;

        public void* __slot15;

        public void* __slot16;

        public void* __slot17;

        public void* __slot18;

        public void* __slot19;

        public delegate* unmanaged<FArchive*, void> Flush;

        public delegate* unmanaged<FArchive*, BOOL> Close;

        public delegate* unmanaged<FArchive*, BOOL> GetError;

        public void* __slot23;

        public void* __slot24;

        public void* __slot25;

        public void* __slot26;

        public void* __slot27;

        public void* __slot28;

        public void* __slot29;
    }

    public static delegate* unmanaged<FArchive*, FArchive*> __ctor { get; } =
        (delegate* unmanaged<FArchive*, FArchive*>)Tera.Resolve(0x7ff69a843f10);

    public static delegate* unmanaged<FArchive*, uint, FArchive*> __vdtor { get; } =
        (delegate* unmanaged<FArchive*, uint, FArchive*>)Tera.Resolve(0x7ff69a845fe0);

    public static delegate* unmanaged<FArchive*, BOOL> AtEnd { get; } =
        (delegate* unmanaged<FArchive*, BOOL>)Tera.Resolve(0x7ff69a846ac0);

    public static delegate* unmanaged<FArchive*, void*, int, FArchive*> ByteOrderSerialize { get; } =
        (delegate* unmanaged<FArchive*, void*, int, FArchive*>)Tera.Resolve(0x7ff69a846b10);

    public static delegate* unmanaged<FArchive*, BOOL> Close { get; } =
        (delegate* unmanaged<FArchive*, BOOL>)Tera.Resolve(0x7ff69a846ba0);

    public static delegate* unmanaged<FArchive*, BOOL> GetError { get; } =
        (delegate* unmanaged<FArchive*, BOOL>)Tera.Resolve(0x7ff69a8488f0);

    public static delegate* unmanaged<FArchive*, void*, int, void> SerializeBits { get; } =
        (delegate* unmanaged<FArchive*, void*, int, void>)Tera.Resolve(0x7ff69a84bf20);

    public static delegate* unmanaged<FArchive*, uint*, uint, void> SerializeInt { get; } =
        (delegate* unmanaged<FArchive*, uint*, uint, void>)Tera.Resolve(0x7ff69a84bf90);

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;

    [FieldOffset(0xc)]
    public int package_version;

    [FieldOffset(0x10)]
    public int network_version;

    [FieldOffset(0x14)]
    public int licensee_version;

    [FieldOffset(0x18)]
    public BOOL is_reader;

    [FieldOffset(0x2c)]
    public BOOL is_persistent;

    [FieldOffset(0x3c)]
    public BOOL has_errors;

    [FieldOffset(0x50)]
    public BOOL force_order_swap;
}
