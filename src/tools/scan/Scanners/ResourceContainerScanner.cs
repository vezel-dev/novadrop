namespace Vezel.Novadrop.Scanners;

internal sealed class ResourceContainerScanner : GameScanner
{
    private static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x44, 0x8b, 0xda,                         // mov r11d, edx
        0x48, 0x8d, 0x1d, null, null, null, null, // lea rbx, [rip + <disp>]
    };

    [SuppressMessage("", "CA1308")]
    public override async Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken)
    {
        var exe = context.Window;
        var offsets = (await exe.SearchAsync(_pattern, 1, cancellationToken)).ToArray();

        if (offsets.Length != 2)
            return false;

        var keys = offsets.Select(off =>
        {
            var dispOff = off + 6;

            // Resolve the RIP displacement in the instruction to an absolute address.
            var keyAddr = exe.ToAddress(dispOff + sizeof(uint)) + exe.Read<uint>(dispOff);

            if (!exe.TryGetOffset(keyAddr, out var keyOff))
                return null;

            var key = new byte[32];

            return exe.TryRead(keyOff, key) ? key : null;
        });

        if (keys.Any(k => k == null))
            return false;

        await File.WriteAllLinesAsync(
            Path.Combine(context.Output.FullName, "ResourceContainerKeys.txt"),
            keys.Select(k => Convert.ToHexString(k!).ToLowerInvariant()),
            cancellationToken);

        return true;
    }
}
