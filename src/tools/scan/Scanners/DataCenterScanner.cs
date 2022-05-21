namespace Vezel.Novadrop.Scanners;

sealed class DataCenterScanner : IScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <disp>], <imm>
    };

    [SuppressMessage("", "CA1308")]
    public void Run(ScanContext context)
    {
        var exe = context.Process.MainModule;

        Console.WriteLine("Searching for data center decryption function...");

        var offsets = exe.Search(_pattern).ToArray();

        if (offsets.Length != 1)
            throw new ApplicationException("Could not find data center decryption function.");

        var off = offsets[0];
        var arrays = new uint[] { 0, 32 }.Select(idx =>
        {
            var baseOff = off + idx;
            var span = (stackalloc uint[4]);

            for (var i = 0; i < span.Length; i++)
                span[i] = exe.Read<uint>(baseOff + 8 * (uint)i + 4);

            return MemoryMarshal.AsBytes(span).ToArray();
        });

        var strArrays = arrays.Select(k => Convert.ToHexString(k).ToLowerInvariant()).ToArray();

        Console.WriteLine($"Found data center key: {strArrays[0]}");
        Console.WriteLine($"Found data center IV: {strArrays[1]}");

        File.WriteAllLines(Path.Combine(context.Output.FullName, "DataCenterKeys.txt"), strArrays);
    }
}
