using Vezel.Novadrop.Interop.Support;

namespace Vezel.Novadrop.Interop;

public static unsafe class S1
{
    public static ref S1Context* Context => ref *(S1Context**)Tera.Resolve(0x7ff69d7b2040);

    public static ref S1LauncherProxy* LauncherProxy => ref *(S1LauncherProxy**)Tera.Resolve(0x7ff69d87de00);

    public static ref S1ServerDataEvent ServerDataEvent => ref *(S1ServerDataEvent*)Tera.Resolve(0x7ff69d4f6840);

    public static delegate* unmanaged<uint, uint, void*> appMalloc { get; } =
        (delegate* unmanaged<uint, uint, void*>)Tera.Resolve(0x7ff69a8977d0);

    public static delegate* unmanaged<void*, uint, uint, void*> appRealloc { get; } =
        (delegate* unmanaged<void*, uint, uint, void*>)Tera.Resolve(0x7ff69a897870);

    public static delegate* unmanaged<void*, void> appFree { get; } =
        (delegate* unmanaged<void*, void>)Tera.Resolve(0x7ff69a897750);
}
