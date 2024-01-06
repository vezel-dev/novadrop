namespace Vezel.Novadrop.Client;

public sealed class ClientMessageServerOptions
{
    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public ImmutableSortedDictionary<int, ClientServerInfo> Servers { get; private set; } =
        ImmutableSortedDictionary<int, ClientServerInfo>.Empty;

    public int LastServerId { get; private set; }

    public Func<int, ReadOnlyMemory<string>, Uri>? WebUriProvider { get; private set; }

    private ClientMessageServerOptions()
    {
    }

    public ClientMessageServerOptions(string accountName, string sessionTicket)
    {
        Check.Null(accountName);
        Check.Null(sessionTicket);

        AccountName = accountName;
        SessionTicket = sessionTicket;
    }

    private ClientMessageServerOptions Clone()
    {
        return new()
        {
            AccountName = AccountName,
            SessionTicket = SessionTicket,
            Servers = Servers,
            LastServerId = LastServerId,
            WebUriProvider = WebUriProvider,
        };
    }

    public ClientMessageServerOptions WithAccountName(string accountName)
    {
        Check.Null(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public ClientMessageServerOptions WithSessionTicket(string sessionTicket)
    {
        Check.Null(sessionTicket);

        var options = Clone();

        options.SessionTicket = sessionTicket;

        return options;
    }

    public ClientMessageServerOptions WithServers(params ClientServerInfo[] servers)
    {
        return WithServers(servers.AsEnumerable());
    }

    public ClientMessageServerOptions WithServers(IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = servers.ToImmutableSortedDictionary(static srv => srv.Id, static srv => srv);

        return options;
    }

    public ClientMessageServerOptions AddServer(ClientServerInfo server)
    {
        Check.Null(server);

        var options = Clone();

        options.Servers = Servers.Add(server.Id, server);

        return options;
    }

    public ClientMessageServerOptions AddServers(params ClientServerInfo[] servers)
    {
        return AddServers(servers.AsEnumerable());
    }

    public ClientMessageServerOptions AddServers(IEnumerable<ClientServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = Servers.AddRange(servers.Select(static srv => KeyValuePair.Create(srv.Id, srv)));

        return options;
    }

    public ClientMessageServerOptions RemoveServer(int id)
    {
        var options = Clone();

        options.Servers = Servers.Remove(id);

        return options;
    }

    public ClientMessageServerOptions RemoveServers(params int[] ids)
    {
        return RemoveServers(ids.AsEnumerable());
    }

    public ClientMessageServerOptions RemoveServers(IEnumerable<int> ids)
    {
        var options = Clone();

        options.Servers = Servers.RemoveRange(ids);

        return options;
    }

    public ClientMessageServerOptions ClearServers()
    {
        return WithServers();
    }

    public ClientMessageServerOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public ClientMessageServerOptions WithWebUriProvider(Func<int, ReadOnlyMemory<string>, Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
