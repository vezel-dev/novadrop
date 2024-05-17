// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe struct S1SceneBase
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public IS1MemoryObject.VirtualFunctionTable IS1MemoryObject;

        public new delegate* unmanaged<S1SceneBase*, S1Type*> GetType;

        public delegate* unmanaged<S1SceneBase*, FString*, FString*> GetName;

        public delegate* unmanaged<S1SceneBase*, S1SceneBase*, void> OnEnter;

        public delegate* unmanaged<S1SceneBase*, S1SceneBase*, void> OnExit;

        public delegate* unmanaged<S1SceneBase*, float, void> Tick;

        public void* __slot7;

        public void* __slot8;
    }

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;
}
