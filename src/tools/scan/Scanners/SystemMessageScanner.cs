namespace Vezel.Novadrop.Scanners;

sealed class SystemMessageScanner : IScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x85, 0xc9,                               // test ecx, eax
        0x78, 0x17,                               // js short 0x17
        0x81, 0xf9, null, null, null, null,       // cmp ecx, <imm>
        0x73, 0x0f,                               // jnb short 0xf
        0x48, 0x63, 0xc1,                         // movsxd rax, ecx
        0x48, 0x8d, 0x0d, null, null, null, null, // lea rcx, [rip + <disp>]
    };

    public unsafe void Run(ScanContext context)
    {
        var process = context.Process;
        var exe = process.MainModule;

        Console.WriteLine("Searching for system message name function...");

        var offsets = exe.Search(_pattern).ToArray();

        if (offsets.Length != 1)
            throw new ApplicationException("Could not find system message name function.");

        var off = offsets[0];
        var count = exe.Read<uint>(off + 6);

        if (count < 4000)
            throw new ApplicationException("Could not read system message count.");

        var dispOff = off + 18;

        // Resolve the RIP displacement in the instruction to an absolute address.
        var tableAddr = exe.ToAddress(dispOff + sizeof(uint)) + exe.Read<uint>(dispOff);

        if (!exe.TryGetOffset(tableAddr, out var tableOff))
            throw new ApplicationException("Could not find system message table.");

        var messages = new Dictionary<string, uint>((int)count);

        for (var i = 0u; i < count; i++)
        {
            if (!exe.TryRead<nuint>(tableOff + (uint)sizeof(nuint) * i, out var strAddr))
                throw new ApplicationException("Could not index system message table.");

            if (!exe.TryGetOffset((NativeAddress)strAddr, out var strOff))
                throw new ApplicationException("Could not find system message name.");

            var sb = new StringBuilder(128);

            for (var j = 0u; ; j++)
            {
                if (!exe.TryRead<char>(strOff + sizeof(char) * j, out var ch))
                    throw new ApplicationException("Could not read system message name.");

                if (ch == '\0')
                    break;

                _ = sb.Append(ch);
            }

            messages.Add(sb.ToString(), i);
        }

        Console.WriteLine($"Found system messages: {count}");

        File.WriteAllLines(
            Path.Combine(context.Output.FullName, "SystemMessages.txt"),
            messages.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}"));
    }
}
