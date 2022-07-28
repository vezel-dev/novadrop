using Vezel.Novadrop.Memory;

namespace Vezel.Novadrop.Patches;

public sealed class TelemetryRemovalPatch : GamePatch
{
    private static readonly ReadOnlyMemory<byte?> _pattern1 = new byte?[]
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

    private static readonly ReadOnlyMemory<byte?> _pattern2 = new byte?[]
    {
        0x48, 0x89, 0x5c, 0x24, 0x10,                   // mov qword ptr [rsp + 0x10], rbx
        0x48, 0x89, 0x74, 0x24, 0x18,                   // mov qword ptr [rsp + 0x18], rsi
        0x48, 0x89, 0x7c, 0x24, 0x20,                   // mov qword ptr [rsp + 0x20], rdi
        0x55,                                           // push rbp
        0x41, 0x54,                                     // push r12
        0x41, 0x55,                                     // push r13
        0x41, 0x56,                                     // push r14
        0x41, 0x57,                                     // push r15
        0x48, 0x8d, 0xac, 0x24, 0xa0, 0xf9, 0xff, 0xff, // lea rbp, [rsp - 0x660]
    };

    private static readonly ReadOnlyMemory<byte?> _pattern3 = new byte?[]
    {
        0x48, 0x89, 0x5c, 0x24, 0x08,                   // mov qword ptr [rsp + 0x8], rbx
        0x48, 0x89, 0x74, 0x24, 0x10,                   // mov qword ptr [rsp + 0x10], rbx
        0x48, 0x89, 0x7c, 0x24, 0x18,                   // mov qword ptr [rsp + 0x18], rbx
        0x55,                                           // push rbp
        0x41, 0x54,                                     // push r12
        0x41, 0x55,                                     // push r13
        0x41, 0x56,                                     // push r14
        0x41, 0x57,                                     // push r15
        0x48, 0x8d, 0xac, 0x24, null, null, null, null, // lea rbp, [rsp + <disp>]
        0x48, 0x81, 0xec, null, null, null, null,       // sub rsp, <imm>
        0x48, 0x8b, 0x05, null, null, null, null,       // mov rax, qword ptr [rip + <disp>]
        0x48, 0x33, 0xc4,                               // xor rax, rsp
        0x48, 0x89, 0x85, null, null, null, null,       // mov qword ptr [rbp + <disp>], rax
        0xe8, null, null, null, null,                   // call <imm>
        0xf7, 0xd8,                                     // neg eax
    };

    private static readonly ReadOnlyMemory<byte> _patch = new byte[]
    {
        0x33, 0xc0, // xor eax, eax
        0xc3,       // ret
    };

    private readonly List<(nuint, ReadOnlyMemory<byte>)> _functions = new();

    public TelemetryRemovalPatch(NativeProcess process)
        : base(process)
    {
    }

    protected override async Task InitializeCoreAsync(CancellationToken cancellationToken)
    {
        var results = await Task.WhenAll(
            new[] { _pattern1, _pattern2, _pattern3 }
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Select(p => Window.SearchAsync(p, cancellationToken)))
            .ConfigureAwait(false);

        foreach (var offsets in results)
        {
            var arr = offsets.ToArray();

            if (arr.Length != 1)
                throw new GamePatchException("Could not locate telemetry functions.");

            var off = arr[0];
            var code = new byte[_patch.Length];

            Window.Read(off, code);

            _functions.Add((off, code));
        }
    }

    protected override void Apply()
    {
        foreach (var (off, _) in _functions)
            Window.Write(off, _patch.Span);
    }

    protected override void Revert()
    {
        foreach (var (off, original) in _functions)
            Window.Write(off, original.Span);
    }
}
