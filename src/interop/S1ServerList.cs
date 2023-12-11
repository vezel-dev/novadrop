using Vezel.Novadrop.Interop.Support;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct S1ServerList
{
    public static delegate* unmanaged<S1ServerList*, void> __dtor { get; } =
        (delegate* unmanaged<S1ServerList*, void>)Tera.Resolve(0x7ff69bcc2500);

    public static delegate* unmanaged<S1ServerList*, S1ServerList*, int> Reset { get; } =
        (delegate* unmanaged<S1ServerList*, S1ServerList*, int>)Tera.Resolve(0x7ff69bccab30);

    [FieldOffset(0x0)]
    public TArray<S1ServerInfo> servers;

    [FieldOffset(0x10)]
    public int last_server_id;

    [FieldOffset(0x14)]
    public S1ServerListSortCriteria sort_criterion;
}
