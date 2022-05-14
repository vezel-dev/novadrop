namespace Vezel.Novadrop.Scanners;

sealed class DataCenterScanner : IScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
        0x41, 0xc7, 0x43, null, null, null, null, null, // mov dword ptr [r11 - <offset>], <value>
    };

    public void Run(ScanContext context)
    {
        var exe = context.Process.MainModule;

        Console.WriteLine("Searching for data center decryption function...");

        var o = exe.Search(_pattern).Cast<nuint?>().FirstOrDefault();

        if (o is not nuint off)
            throw new ApplicationException("Could not find data center decryption function.");

        var decoder = Iced.Intel.Decoder.Create(64, new MemoryWindowCodeReader(exe.Slice(off)));

        byte[]? ReadKey()
        {
            using var stream = new MemoryStream(16);
            using var writer = new BinaryWriter(stream);

            for (var i = 0; i < 4; i++)
            {
                decoder.Decode(out var insn);

                if (insn.Code != Code.Mov_rm32_imm32 || insn.MemoryBase != Register.R11)
                    return null;

                writer.Write(insn.Immediate32);
            }

            return stream.ToArray();
        }

        if (ReadKey() is not byte[] key)
            throw new ApplicationException("Could not find data center key.");

        if (ReadKey() is not byte[] iv)
            throw new ApplicationException("Could not find data center IV.");

        static string StringizeKey(byte[] key)
        {
            return string.Join(" ", key.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }

        Console.WriteLine($"Found data center key: {StringizeKey(key)}");
        Console.WriteLine($"Found data center IV: {StringizeKey(iv)}");

        File.WriteAllLines(Path.Combine(context.Output.FullName, "DataCenterKeys.txt"), new[]
        {
            string.Join(", ", key.Select(x => $"0x{x:x2}")),
            string.Join(", ", iv.Select(x => $"0x{x:x2}")),
        });
    }
}
