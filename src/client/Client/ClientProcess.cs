namespace Vezel.Novadrop.Client;

public sealed class ClientProcess : GameProcess
{
    // Represents a TERA.exe process from the perspective of a Tl.exe-compatible process.

    public ClientProcessOptions Options { get; }

    public ClientProcess(ClientProcessOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;
    }

    protected override void GetWindowConfiguration(out string className, out string windowName)
    {
        className = "LAUNCHER_CLASS";
        windowName = "LAUNCHER_WINDOW";
    }

    protected override void GetProcessConfiguration(out string fileName, out string[] arguments)
    {
        fileName = Options.FileName;
        arguments = Options.Language is string lang ? new[] { $"-LANGUAGEEXT={lang}" } : Array.Empty<string>();
    }

    protected override (nuint Id, ReadOnlyMemory<byte> Payload)? HandleWindowMessage(
        nuint id, ReadOnlySpan<byte> payload)
    {
        var opts = Options;

        ReadOnlyMemory<byte> SerializeServerList()
        {
            using var ms = new MemoryStream(ushort.MaxValue);

            var csl = new ClientServerList
            {
                LastServerId = (uint)opts.LastServerId,
            };

            var utf16 = Encoding.Unicode;

            foreach (var (_, srv) in opts.Servers)
                csl.Servers.Add(new()
                {
                    Id = (uint)srv.Id,
                    RawName = utf16.GetBytes(srv.RawName),
                    Category = utf16.GetBytes(srv.Category),
                    Name = utf16.GetBytes(srv.Name),
                    Queue = utf16.GetBytes(srv.Queue),
                    Population = utf16.GetBytes(srv.Population),
                    Address = BinaryPrimitives.ReadUInt32BigEndian(srv.Address.GetAddressBytes()),
                    Port = (uint)srv.Port,
                    Available = srv.IsAvailable ? 1u : 0,
                    UnavailableMessage = utf16.GetBytes(srv.UnavailableMessage),
                });

            Serializer.Serialize(ms, csl);

            return ms.ToArray();
        }

        return id switch
        {
            1 => (2, Encoding.Unicode.GetBytes(opts.AccountName)),
            3 => (4, Encoding.UTF8.GetBytes(opts.Ticket)),
            5 => (6, SerializeServerList()),
            7 => null,
            25 => null,
            26 => null,
            1000 => null,
            1001 => null,
            1002 => null,
            1003 => null,
            1004 => null,
            1005 => null,
            1006 => null,
            1007 => null,
            1008 => null,
            1009 => null,
            1010 => null,
            1011 => null,
            1012 => null,
            1013 => null,
            1015 => null,
            1016 => null,
            1020 => null,
            1021 => null,
            1025 => null,
            1027 => null,
            _ => null,
        };
    }
}
