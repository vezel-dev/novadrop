namespace Vezel.Novadrop.Scanners;

sealed class ClientVersionScanner : GameScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <disp>]
        0xba, 0x14, 0x00, 0x00, 0x00,       // mov edx, 0x14
        0x89, 0x05, null, null, null, null, // mov dword ptr [rip + <disp>], eax
        0x45, 0x33, 0xc0,                   // xor r8d, r8d
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <disp>]
    };

    public override async Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken)
    {
        var exe = context.Window;
        var offsets = (await exe.SearchAsync(_pattern, 1, cancellationToken)).ToArray();

        if (offsets.Length != 1)
            return false;

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
            return false;

        await File.WriteAllLinesAsync(
            Path.Combine(context.Output.FullName, "ClientVersions.txt"),
            vers.Select(v => v.ToString(CultureInfo.InvariantCulture)),
            cancellationToken);

        return true;
    }
}
