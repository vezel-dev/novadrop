namespace Vezel.Novadrop.Scanners;

sealed class ClientVersionScanner : IScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <disp>]
        0xba, 0x14, 0x00, 0x00, 0x00,       // mov edx, 0x14
        0x89, 0x05, null, null, null, null, // mov dword ptr [rip + <disp>], eax
        0x45, 0x33, 0xc0,                   // xor r8d, r8d
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <disp>]
    };

    public async Task RunAsync(ScanContext context)
    {
        var exe = context.Process.MainModule;

        Console.WriteLine("Searching for client version reporting function...");

        var offsets = (await exe.SearchAsync(_pattern)).ToArray();

        if (offsets.Length != 1)
            throw new ApplicationException("Could not find client version reporting function.");

        var off = offsets[0];
        var vers = new uint[] { 2, 22 }.Select(idx =>
        {
            var dispOff = off + idx;

            // Resolve the RIP displacement in the instruction to an absolute address.
            var verAddr = exe.ToAddress(dispOff + sizeof(uint)) + exe.Read<uint>(dispOff);

            return exe.TryGetOffset(verAddr, out var verOff)
                ? exe.TryRead<int>(verOff, out var ver)
                    ? ver
                    : 0
                : 0;
        });

        if (vers.Any(v => v == 0))
            throw new ApplicationException("Could not read client version values.");

        // The first is game message table version, the second is system message table version.
        foreach (var ver in vers)
            Console.WriteLine($"Found client version: {ver}");

        await File.WriteAllLinesAsync(
            Path.Combine(context.Output.FullName, "ClientVersions.txt"),
            vers.Select(v => v.ToString(CultureInfo.InvariantCulture)));
    }
}
