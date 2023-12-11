using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10004)]
public unsafe struct S1ClientSocketBuffer
{
    [InlineArray(0x10000)]
    public struct InlineArray_data
    {
        private byte _element;
    }

    public static delegate* unmanaged<S1ClientSocketBuffer*, S1ClientSocketBuffer*> __ctor { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, S1ClientSocketBuffer*>)Tera.Resolve(0x7ff69baa27a0);

    public static delegate* unmanaged<S1ClientSocketBuffer*, int, int> Consume { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, int, int>)Tera.Resolve(0x7ff69babc170);

    public static delegate* unmanaged<S1ClientSocketBuffer*, int> GetPosition { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, int>)Tera.Resolve(0x7ff69bab21f0);

    public static delegate* unmanaged<S1ClientSocketBuffer*, byte*, int, BOOL> Write { get; } =
        (delegate* unmanaged<S1ClientSocketBuffer*, byte*, int, BOOL>)Tera.Resolve(0x7ff69bacacd0);

    [FieldOffset(0x0)]
    public InlineArray_data data;

    [FieldOffset(0x10000)]
    public int position;
}
