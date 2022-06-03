using Vezel.Novadrop.Memory;

namespace Vezel.Novadrop.Patches;

public sealed class TelemetryRemovalPatch : GamePatch
{
    static readonly ReadOnlyMemory<byte?> _pattern1 = new byte?[]
    {
        0x48, 0x89, 0x5c, 0x24, 0x08,                   // mov qword ptr [rsp + 0x8], rbx
        0x48, 0x89, 0x74, 0x24, 0x18,                   // mov qword ptr [rsp + 0x18], rsi
        0x48, 0x89, 0x7c, 0x24, 0x20,                   // mov qword ptr [rsp + 0x20], rdi
        0x55,                                           // push rbp
        0x41, 0x54,                                     // push r12
        0x41, 0x55,                                     // push r13
        0x41, 0x56,                                     // push r14
        0x41, 0x57,                                     // push r15
        0x48, 0x8d, 0xac, 0x24, 0xf0, 0xfb, 0xff, 0xff, // lea rbp, [rsp - 0x410]
    };

    nuint _offset;

    byte _original;

    public TelemetryRemovalPatch(NativeProcess process)
        : base(process)
    {
    }

    protected override async Task<bool> InitializeAsync(CancellationToken cancellationToken)
    {
        var offsets = (await Executable.SearchAsync(_pattern1, cancellationToken).ConfigureAwait(false)).ToArray();

        if (offsets.Length != 1)
            return false;

        _offset = offsets[0];
        _original = Executable.Read<byte>(_offset);

        // TODO: Also remove crash telemetry.

        return true;
    }

    protected override Task<bool> ApplyAsync(CancellationToken cancellationToken)
    {
        Executable.Write(_offset, (byte)0xc3); // ret

        return Task.FromResult(true);
    }

    protected override Task<bool> RevertAsync(CancellationToken cancellationToken)
    {
        Executable.Write(_offset, _original);

        return Task.FromResult(true);
    }
}
