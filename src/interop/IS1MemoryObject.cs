// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct IS1MemoryObject
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public delegate* unmanaged<IS1MemoryObject*, uint, IS1MemoryObject*> __vdtor;

        public delegate* unmanaged<IS1MemoryObject*, int> GetSize;
    }

    public static delegate* unmanaged<IS1MemoryObject*, uint, IS1MemoryObject*> __vdtor { get; } =
        (delegate* unmanaged<IS1MemoryObject*, uint, IS1MemoryObject*>)Tera.Resolve(0x7ff69b4579e0);

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
