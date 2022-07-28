namespace Vezel.Novadrop.Scanners;

internal sealed class DataCenterScanner : GameScanner
{
    private static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 + <disp>], <imm>
    };

    [SuppressMessage("", "CA1308")]
    public override async Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken)
    {
        var exe = context.Window;
        var offsets = (await exe.SearchAsync(_pattern, 1, cancellationToken)).ToArray();

        if (offsets.Length != 1)
            return false;

        var off = offsets[0];
        var arrays = new uint[] { 0, 32 }.Select(idx =>
        {
            var baseOff = off + idx;
            var span = (stackalloc uint[4]);

            for (var i = 0; i < span.Length; i++)
                span[i] = exe.Read<uint>(baseOff + 8 * (uint)i + 4);

            return MemoryMarshal.AsBytes(span).ToArray();
        });

        await File.WriteAllLinesAsync(
            Path.Combine(context.Output.FullName, "DataCenterKeys.txt"),
            arrays.Select(k => Convert.ToHexString(k).ToLowerInvariant()),
            cancellationToken);

        return true;
    }
}
