using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x64)]
public unsafe struct S1CommandQueue
{
    public static delegate* unmanaged<S1CommandQueue*, byte*, void> EnqueuePacket { get; } =
        (delegate* unmanaged<S1CommandQueue*, byte*, void>)Tera.Resolve(0x7ff69baac250);

    public static delegate* unmanaged<S1CommandQueue*, void> RunCommands { get; } =
        (delegate* unmanaged<S1CommandQueue*, void>)Tera.Resolve(0x7ff69baaa560);

    public static delegate* unmanaged<S1CommandQueue*, byte*, BOOL> RunPacketHandler { get; } =
        (delegate* unmanaged<S1CommandQueue*, byte*, BOOL>)Tera.Resolve(0x7ff69baad410);
}
