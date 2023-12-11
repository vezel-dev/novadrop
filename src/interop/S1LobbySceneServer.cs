using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x108)]
public unsafe struct S1LobbySceneServer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VirtualFunctionTable
    {
        public S1SceneBase.VirtualFunctionTable S1SceneBase;
    }

    public static delegate* unmanaged<S1LobbySceneServer*, S1LobbySceneServer*> __ctor { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, S1LobbySceneServer*>)Tera.Resolve(0x7ff69bd26010);

    public static delegate* unmanaged<S1LobbySceneServer*, uint, S1LobbySceneServer*> __vdtor { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, uint, S1LobbySceneServer*>)Tera.Resolve(0x7ff69bd27e00);

    public static delegate* unmanaged<S1LobbySceneServer*, S1SceneBase*, void> OnEnter { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, S1SceneBase*, void>)Tera.Resolve(0x7ff69bd3b720);

    public static delegate* unmanaged<S1LobbySceneServer*, S1SceneBase*, void> OnExit { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, S1SceneBase*, void>)Tera.Resolve(0x7ff69bd3b860);

    public static delegate* unmanaged<S1LobbySceneServer*, FString*, FString*> GetName { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, FString*, FString*>)Tera.Resolve(0x7ff69bd35d90);

    public static new delegate* unmanaged<S1LobbySceneServer*, S1Type*> GetType { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, S1Type*>)Tera.Resolve(0x7ff69bd361b0);

    public static delegate* unmanaged<S1LobbySceneServer*, in_addr, void> SnoopLoginArbiter { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, in_addr, void>)Tera.Resolve(0x7ff69bd409c0);

    public static delegate* unmanaged<S1LobbySceneServer*, float, void> Tick { get; } =
        (delegate* unmanaged<S1LobbySceneServer*, float, void>)Tera.Resolve(0x7ff69bd49380);

    [FieldOffset(0x0)]
    public IS1MemoryObject IS1MemoryObject;

    [FieldOffset(0x0)]
    public S1SceneBase S1SceneBase;

    [FieldOffset(0x0)]
    public VirtualFunctionTable* VFT;

    [FieldOffset(0x8)]
    public S1ServerList server_list;
}
