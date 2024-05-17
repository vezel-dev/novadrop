// SPDX-License-Identifier: 0BSD

using Vezel.Novadrop.Interop.Support;
using Vezel.Novadrop.Interop.System;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0xe0)]
public unsafe struct S1LauncherProxy
{
    public static delegate* unmanaged<S1LauncherProxy*, S1LauncherProxy*> __ctor { get; } =
        (delegate* unmanaged<S1LauncherProxy*, S1LauncherProxy*>)Tera.Resolve(0x7ff69bcc1ad0);

    public static delegate* unmanaged<S1LauncherProxy*, void> __dtor { get; } =
        (delegate* unmanaged<S1LauncherProxy*, void>)Tera.Resolve(0x7ff69bcc29d0);

    public static delegate* unmanaged<S1LauncherProxy*, void> Dispose { get; } =
        (delegate* unmanaged<S1LauncherProxy*, void>)Tera.Resolve(0x7ff69bccc1e0);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleAccountNameResponse { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bcd9ad0);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleCreateRoomResult { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bce7420);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleJoinRoomResult { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bce7300);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleLeaveRoomResult { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bce7390);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleServerListResponse { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bcd91c0);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleSessionTicketResponse { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bcd99e0);

    public static delegate* unmanaged<S1LauncherProxy*, byte*, int, void> HandleWebUrlResponse { get; } =
        (delegate* unmanaged<S1LauncherProxy*, byte*, int, void>)Tera.Resolve(0x7ff69bcd4800);

    public static delegate* unmanaged<S1LauncherProxy*, void> Initialize { get; } =
        (delegate* unmanaged<S1LauncherProxy*, void>)Tera.Resolve(0x7ff69bcd0390);

    public static delegate* unmanaged<S1LauncherProxy*, FString*> SendAccountNameRequest { get; } =
        (delegate* unmanaged<S1LauncherProxy*, FString*>)Tera.Resolve(0x7ff69bccfa90);

    public static delegate* unmanaged<S1LauncherProxy*, FString*, S1LauncherEvent, void> SendCrashEvent { get; } =
        (delegate* unmanaged<S1LauncherProxy*, FString*, S1LauncherEvent, void>)Tera.Resolve(0x7ff69bcdea20);

    public static delegate* unmanaged<S1LauncherProxy*, S1LauncherEvent, byte*, int, int> SendEvent { get; } =
        (delegate* unmanaged<S1LauncherProxy*, S1LauncherEvent, byte*, int, int>)Tera.Resolve(0x7ff69bcdeda0);

    public static delegate* unmanaged<S1LauncherProxy*, int, S1ExitReason, void> SendExitEvent { get; } =
        (delegate* unmanaged<S1LauncherProxy*, int, S1ExitReason, void>)Tera.Resolve(0x7ff69bcdebb0);

    public static delegate* unmanaged<S1LauncherProxy*, S1LauncherEvent, byte*, int, void> SendGameEvent { get; } =
        (delegate* unmanaged<S1LauncherProxy*, S1LauncherEvent, byte*, int, void>)Tera.Resolve(0x7ff69bcdecf0);

    public static delegate* unmanaged<S1LauncherProxy*, int, void> SendOpenWebsiteCommand { get; } =
        (delegate* unmanaged<S1LauncherProxy*, int, void>)Tera.Resolve(0x7ff69bcd9e60);

    public static delegate* unmanaged<S1LauncherProxy*, BOOL, int, void> SendServerListRequest { get; } =
        (delegate* unmanaged<S1LauncherProxy*, BOOL, int, void>)Tera.Resolve(0x7ff69bcdd2f0);

    public static delegate* unmanaged<S1LauncherProxy*, TArray<byte>*> SendSessionTicketRequest { get; } =
        (delegate* unmanaged<S1LauncherProxy*, TArray<byte>*>)Tera.Resolve(0x7ff69bccf390);

    public static delegate* unmanaged<S1LauncherProxy*, int, char*, void> SendWebUrlRequest { get; } =
        (delegate* unmanaged<S1LauncherProxy*, int, char*, void>)Tera.Resolve(0x7ff69bcd9f80);

    [FieldOffset(0x0)]
    public HWND game_window;

    [FieldOffset(0x8)]
    public HWND launcher_window;

    [FieldOffset(0x70)]
    public BOOL exited;

    [FieldOffset(0x74)]
    public FString account_name;

    [FieldOffset(0x88)]
    public TArray<byte> session_ticket;

    [FieldOffset(0xa0)]
    public S1ServerList server_list;
}
