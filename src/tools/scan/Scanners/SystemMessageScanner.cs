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

    public async Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken)
    {
        var process = context.Process;
        var exe = process.MainModule;

        var offsets = (await exe.SearchAsync(_pattern, cancellationToken)).ToArray();

        if (offsets.Length != 1)
            return false;

        var off = offsets[0];
        var count = exe.Read<uint>(off + 6);

        if (count < 4000)
            return false;

        var dispOff = off + 18;

        // Resolve the RIP displacement in the instruction to an absolute address.
        var tableAddr = exe.ToAddress(dispOff + sizeof(uint)) + exe.Read<uint>(dispOff);

        if (!exe.TryGetOffset(tableAddr, out var tableOff))
            return false;

        var messages = new Dictionary<string, uint>((int)count);

        for (var i = 0u; i < count; i++)
        {
            if (!exe.TryRead<nuint>(tableOff + (uint)Unsafe.SizeOf<nuint>() * i, out var strAddr))
                return false;

            if (!exe.TryGetOffset((NativeAddress)strAddr, out var strOff))
                return false;

            var sb = new StringBuilder(128);

            for (var j = 0u; ; j++)
            {
                if (!exe.TryRead<char>(strOff + sizeof(char) * j, out var ch))
                    return false;

                if (ch == '\0')
                    break;

                _ = sb.Append(ch);
            }

            messages.Add(sb.ToString(), i);
        }

        await File.WriteAllLinesAsync(
            Path.Combine(context.Output.FullName, "SystemMessages.txt"),
            messages.OrderBy(x => x.Key).Select(x => $"{x.Key} = {x.Value}"),
            cancellationToken);

        return true;
    }
}
