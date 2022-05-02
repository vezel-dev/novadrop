namespace Vezel.Novadrop.Scanners;

sealed class ClientVersionScanner : IScanner
{
    static readonly ReadOnlyMemory<byte?> _pattern = new byte?[]
    {
        0x74, 0x72,                         // je short 0x72
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <offset>]
        0xba, 0x14, 0x00, 0x00, 0x00,       // mov edx, 0x14
        0x89, 0x05, null, null, null, null, // mov dword ptr [rip + <offset>], eax
        0x45, 0x33, 0xc0,                   // xor r8d, r8d
        0x8b, 0x05, null, null, null, null, // mov eax, dword ptr [rip + <offset>]
    };

    public void Run(ScanContext context)
    {
        var module = context.Process.MainModule;

        Console.WriteLine("Searching for client version reporting function...");

        var o = module.Search(_pattern).Cast<nuint?>().FirstOrDefault();

        if (o is not nuint off)
            throw new ApplicationException("Could not find client version reporting function.");

        int? ReadVersion(nuint offset)
        {
            return module.TryGetOffset(
                module.ToAddress(offset + sizeof(uint)) + module.Read<uint>(offset),
                out var verOff)
                ? module.Read<int>(verOff)
                : null;
        }

        var v1 = ReadVersion(off + 4);

        if (v1 is not int ver1)
            throw new ApplicationException("Could not read the first client version value.");

        var v2 = ReadVersion(off + 24);

        if (v2 is not int ver2 || ver2 == 0)
            throw new ApplicationException("Could not read the second client version value.");

        Console.WriteLine($"Found client versions: {ver1}, {ver2}");

        File.WriteAllLines(Path.Combine(context.Output.FullName, "ClientVersions.txt"), new[]
        {
            ver1.ToString(CultureInfo.InvariantCulture),
            ver2.ToString(CultureInfo.InvariantCulture),
        });
    }
}
