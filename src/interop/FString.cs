using Vezel.Novadrop.Interop.Support;

namespace Vezel.Novadrop.Interop;

[StructLayout(LayoutKind.Explicit, Size = 0x14)]
public unsafe struct FString
{
    public static delegate* unmanaged<FString*, char*, FString*> __ctor { get; } =
        (delegate* unmanaged<FString*, char*, FString*>)Tera.Resolve(0x7ff69a844310);

    public static delegate* unmanaged<FString*, FString*, FString*> __ctor2 { get; } =
        (delegate* unmanaged<FString*, FString*, FString*>)Tera.Resolve(0x7ff69a844140);

    [FieldOffset(0x0)]
    public TArray<char> TArray_char;
}
