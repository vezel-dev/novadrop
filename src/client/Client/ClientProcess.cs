namespace Vezel.Novadrop.Client;

public sealed class ClientProcess : GameProcess
{
    // Represents a TERA.exe process from the perspective of a Tl.exe-compatible process.

    public event Action<int>? GameStarted;

    public event Action? ServerListRequested;

    public event Action? AccountNameRequested;

    public event Action? SessionTicketRequested;

    public event Action? LobbyEntered;

    public event Action<string>? WorldEntered;

    public event ReadOnlySpanAction<string, int>? WebUriRequested;

    public event Action<GameEvent>? GameEventOccurred;

    public event Action<int>? GameExited;

    public event Action<string>? GameCrashed;

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
        var utf16 = Encoding.Unicode;

        (nuint, ReadOnlyMemory<byte>) HandleAccountNameRequest()
        {
            AccountNameRequested?.Invoke();

            return (0x2, Encoding.Unicode.GetBytes(opts.AccountName));
        }

        (nuint, ReadOnlyMemory<byte>) HandleSessionTicketRequest()
        {
            SessionTicketRequested?.Invoke();

            return (0x4, Encoding.UTF8.GetBytes(opts.SessionTicket));
        }

        (nuint, ReadOnlyMemory<byte>) HandleServerListRequest(ReadOnlySpan<byte> payload)
        {
            ServerListRequested?.Invoke();

            using var ms = new MemoryStream(ushort.MaxValue);

            var csl = new ProtoBufServerList
            {
                LastServerId = (uint)opts.LastServerId,
                SortCriterion = BinaryPrimitives.ReadUInt32LittleEndian(payload),
            };

            foreach (var srv in opts.Servers.Values.OrderBy(s => s.Id))
                csl.Servers.Add(new()
                {
                    Id = (uint)srv.Id,
                    Title = utf16.GetBytes(srv.Name),
                    Category = utf16.GetBytes(srv.Category),
                    Name = utf16.GetBytes(srv.Title),
                    Queue = utf16.GetBytes(srv.Queue),
                    Population = utf16.GetBytes(srv.Population),
                    Address = srv.Address is IPAddress addr
                        ? BinaryPrimitives.ReadUInt32BigEndian(addr.GetAddressBytes())
                        : 0,
                    Port = (uint)srv.Port,
                    Available = srv.IsAvailable ? 1u : 0,
                    UnavailableMessage = utf16.GetBytes(srv.UnavailableMessage),
                    Host = srv.Host is string host ? utf16.GetBytes(host) : null,
                });

            Serializer.Serialize(ms, csl);

            return (0x6, ms.ToArray());
        }

        (nuint, ReadOnlyMemory<byte>)? HandleEnterLobbyOrWorld(ReadOnlySpan<byte> payload)
        {
            if (payload.IsEmpty)
                LobbyEntered?.Invoke();
            else
                WorldEntered?.Invoke(utf16.GetString(payload)[..^1]); // Strip NUL terminator.

            return null;
        }

        (nuint, ReadOnlyMemory<byte>)? HandleWebUriRequest(ReadOnlySpan<byte> payload)
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(payload);
            var args = utf16.GetString(payload[sizeof(int)..]).TrimEnd('\0').Split(',');

            WebUriRequested?.Invoke(args, id);

            if (Options.WebUriProvider?.Invoke(id, args) is not Uri uri)
                return null;

            if (!uri.IsAbsoluteUri)
                throw new InvalidOperationException();

            var abs = uri.AbsoluteUri;
            var reply = new byte[sizeof(int) + utf16.GetByteCount(abs) + sizeof(char)]; // Add NUL terminator.

            BinaryPrimitives.WriteInt32LittleEndian(reply, id);
            _ = utf16.GetBytes(abs, reply.AsSpan(sizeof(int)));

            return (0x1b, reply);
        }

        (nuint, ReadOnlyMemory<byte>)? HandleGameStart(ReadOnlySpan<byte> payload)
        {
            GameStarted?.Invoke(BinaryPrimitives.ReadInt32LittleEndian(payload));

            return null;
        }

        (nuint, ReadOnlyMemory<byte>)? HandleGameEvent()
        {
            GameEventOccurred?.Invoke((GameEvent)id);

            return null;
        }

        (nuint, ReadOnlyMemory<byte>)? HandleGameExit(ReadOnlySpan<byte> payload)
        {
            GameExited?.Invoke(BinaryPrimitives.ReadInt32LittleEndian(payload[(sizeof(int) * 2)..sizeof(int)]));

            return null;
        }

        (nuint, ReadOnlyMemory<byte>)? HandleGameCrash(ReadOnlySpan<byte> payload)
        {
            GameCrashed?.Invoke(utf16.GetString(payload).Trim());

            return null;
        }

        return id switch
        {
            0x1 => HandleAccountNameRequest(),
            0x3 => HandleSessionTicketRequest(),
            0x5 => HandleServerListRequest(payload),
            0x7 => HandleEnterLobbyOrWorld(payload),
            0x8 => null,
            0xa => null,
            0xc => null,
            0x13 => null,
            0x14 => null,
            0x15 => null,
            0x19 => null,
            0x1a => HandleWebUriRequest(payload),
            0x1c => null,
            0x3e8 => HandleGameStart(payload),
            >= 0x3e9 and <= 0x3f8 => HandleGameEvent(),
            0x3fc => HandleGameExit(payload),
            0x3fd => HandleGameCrash(payload),
            0x3fe => null,
            0x3ff => null,
            0x400 => null,
            0x401 => null,
            0x403 => null,
            _ => null,
        };
    }
}
