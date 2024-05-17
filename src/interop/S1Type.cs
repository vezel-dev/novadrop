// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe struct S1Type
{
    public static delegate* unmanaged<S1Type*, S1Type*, BOOL> IsSubtypeOf { get; } =
        (delegate* unmanaged<S1Type*, S1Type*, BOOL>)Tera.Resolve(0x7ff69b4142f0);

    [FieldOffset(0x0)]
    public char* name;

    [FieldOffset(0x8)]
    public S1Type* @base;
}
