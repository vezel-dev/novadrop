namespace Vezel.Novadrop.Client;

public sealed class ClientProcessOptions
{
    public string FileName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public string? Language { get; private set; }

    public IReadOnlyDictionary<int, ClientServerInfo> Servers { get; private set; } = null!;

    public int LastServerId { get; private set; }

    public Func<int, string[], Uri>? WebUriProvider { get; private set; }

    ClientProcessOptions()
    {
    }

    public ClientProcessOptions(
        string fileName, string accountName, string sessionTicket, IEnumerable<ClientServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentNullException.ThrowIfNull(sessionTicket);
        ArgumentNullException.ThrowIfNull(servers);
        _ = servers.Any() ? true : throw new ArgumentException(null, nameof(servers));
        _ = servers.All(s => s != null) ? true : throw new ArgumentException(null, nameof(servers));

        FileName = fileName;
        AccountName = accountName;
        SessionTicket = sessionTicket;
        Servers = servers.ToImmutableDictionary(s => s.Id);
    }

    ClientProcessOptions Clone()
    {
        return new()
        {
            FileName = FileName,
            AccountName = AccountName,
            SessionTicket = SessionTicket,
            Language = Language,
            Servers = Servers,
            LastServerId = LastServerId,
            WebUriProvider = WebUriProvider,
        };
    }

    public ClientProcessOptions WithFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var options = Clone();

        options.FileName = fileName;

        return options;
    }

    public ClientProcessOptions WithAccountName(string accountName)
    {
        ArgumentNullException.ThrowIfNull(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public ClientProcessOptions WithSessionTicket(string sessionTicket)
    {
        ArgumentNullException.ThrowIfNull(sessionTicket);

        var options = Clone();

        options.SessionTicket = sessionTicket;

        return options;
    }

    public ClientProcessOptions WithLanguage(string? language)
    {
        var options = Clone();

        options.Language = language;

        return options;
    }

    public ClientProcessOptions WithServers(IEnumerable<ClientServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(servers);
        _ = servers.Any() ? true : throw new ArgumentException(null, nameof(servers));
        _ = servers.All(s => s != null) ? true : throw new ArgumentException(null, nameof(servers));

        var options = Clone();

        options.Servers = servers.ToImmutableDictionary(s => s.Id);

        return options;
    }

    public ClientProcessOptions WithLastServerId(int lastServerId)
    {
        _ = lastServerId > 0 ? true : throw new ArgumentOutOfRangeException(nameof(lastServerId));

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public ClientProcessOptions WithWebUriProvider(Func<int, string[], Uri>? webUriProvider)
    {
        ArgumentNullException.ThrowIfNull(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
