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

    private ClientProcessOptions()
    {
    }

    [SuppressMessage("", "CA1851")]
    public ClientProcessOptions(
        string fileName, string accountName, string sessionTicket, IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(fileName);
        Check.Null(accountName);
        Check.Null(sessionTicket);
        Check.Null(servers);
        Check.Argument(servers.Any(), servers);
        Check.ForEach(servers, srv => Check.Argument(srv != null, servers));

        FileName = fileName;
        AccountName = accountName;
        SessionTicket = sessionTicket;
        Servers = servers.ToImmutableDictionary(s => s.Id);
    }

    private ClientProcessOptions Clone()
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
        Check.Null(fileName);

        var options = Clone();

        options.FileName = fileName;

        return options;
    }

    public ClientProcessOptions WithAccountName(string accountName)
    {
        Check.Null(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public ClientProcessOptions WithSessionTicket(string sessionTicket)
    {
        Check.Null(sessionTicket);

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

    [SuppressMessage("", "CA1851")]
    public ClientProcessOptions WithServers(IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(servers);
        Check.Argument(servers.Any(), servers);
        Check.ForEach(servers, srv => Check.Argument(srv != null, servers));

        var options = Clone();

        options.Servers = servers.ToImmutableDictionary(s => s.Id);

        return options;
    }

    public ClientProcessOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public ClientProcessOptions WithWebUriProvider(Func<int, string[], Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
