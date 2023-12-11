using Vezel.Novadrop.Interop.Support;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x128)]
public unsafe struct S1DataDB
{
    public static delegate* unmanaged<S1DataDB*, void> Initialize { get; } =
        (delegate* unmanaged<S1DataDB*, void>)Tera.Resolve(0x7ff69bb19f30);

    public static delegate* unmanaged<FArchive*, FArchive*> UnpackArchive { get; } =
        (delegate* unmanaged<FArchive*, FArchive*>)Tera.Resolve(0x7ff69b884ac0);
}
