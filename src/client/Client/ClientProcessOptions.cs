namespace Vezel.Novadrop.Client;

public sealed class ClientProcessOptions
{
    public string FileName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public string? Language { get; private set; }

    public ImmutableSortedDictionary<int, ClientServerInfo> Servers { get; private set; } =
        ImmutableSortedDictionary<int, ClientServerInfo>.Empty;

    public int LastServerId { get; private set; }

    public Func<int, ReadOnlyMemory<string>, Uri>? WebUriProvider { get; private set; }

    private ClientProcessOptions()
    {
    }

    public ClientProcessOptions(string fileName, string accountName, string sessionTicket)
    {
        Check.Null(fileName);
        Check.Null(accountName);
        Check.Null(sessionTicket);

        FileName = fileName;
        AccountName = accountName;
        SessionTicket = sessionTicket;
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

    public ClientProcessOptions WithServers(params ClientServerInfo[] servers)
    {
        return WithServers(servers.AsEnumerable());
    }

    public ClientProcessOptions WithServers(IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = servers.ToImmutableSortedDictionary(srv => srv.Id, srv => srv);

        return options;
    }

    public ClientProcessOptions AddServer(ClientServerInfo server)
    {
        Check.Null(server);

        var options = Clone();

        options.Servers = Servers.Add(server.Id, server);

        return options;
    }

    public ClientProcessOptions AddServers(params ClientServerInfo[] servers)
    {
        return AddServers(servers.AsEnumerable());
    }

    public ClientProcessOptions AddServers(IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = Servers.AddRange(servers.Select(srv => KeyValuePair.Create(srv.Id, srv)));

        return options;
    }

    public ClientProcessOptions RemoveServer(int id)
    {
        var options = Clone();

        options.Servers = Servers.Remove(id);

        return options;
    }

    public ClientProcessOptions RemoveServers(params int[] ids)
    {
        return RemoveServers(ids.AsEnumerable());
    }

    public ClientProcessOptions RemoveServers(IEnumerable<int> ids)
    {
        var options = Clone();

        options.Servers = Servers.RemoveRange(ids);

        return options;
    }

    public ClientProcessOptions ClearServers()
    {
        return WithServers();
    }

    public ClientProcessOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public ClientProcessOptions WithWebUriProvider(Func<int, ReadOnlyMemory<string>, Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
