namespace Vezel.Novadrop.Client;

public sealed class LauncherProcessOptions
{
    public string FileName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public Uri ServerListUri { get; private set; } = null!;

    public ImmutableSortedDictionary<int, LauncherServerInfo> Servers { get; private set; } =
        ImmutableSortedDictionary<int, LauncherServerInfo>.Empty;

    public int LastServerId { get; private set; }

    public Func<int, ReadOnlyMemory<string>, Uri>? WebUriProvider { get; private set; }

    private LauncherProcessOptions()
    {
    }

    public LauncherProcessOptions(string fileName, string accountName, string sessionTicket, Uri serverListUri)
    {
        Check.Null(fileName);
        Check.Null(accountName);
        Check.Null(sessionTicket);
        Check.Null(serverListUri);
        Check.Argument(serverListUri.IsAbsoluteUri, serverListUri);

        FileName = fileName;
        AccountName = accountName;
        SessionTicket = sessionTicket;
        ServerListUri = serverListUri;
    }

    private LauncherProcessOptions Clone()
    {
        return new()
        {
            FileName = FileName,
            AccountName = AccountName,
            SessionTicket = SessionTicket,
            ServerListUri = ServerListUri,
            Servers = Servers,
            LastServerId = LastServerId,
            WebUriProvider = WebUriProvider,
        };
    }

    public LauncherProcessOptions WithFileName(string fileName)
    {
        Check.Null(fileName);

        var options = Clone();

        options.FileName = fileName;

        return options;
    }

    public LauncherProcessOptions WithAccountName(string accountName)
    {
        Check.Null(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public LauncherProcessOptions WithSessionTicket(string sessionTicket)
    {
        Check.Null(sessionTicket);

        var options = Clone();

        options.SessionTicket = sessionTicket;

        return options;
    }

    public LauncherProcessOptions WithServerListUri(Uri serverListUri)
    {
        Check.Null(serverListUri);
        Check.Argument(serverListUri.IsAbsoluteUri, serverListUri);

        var options = Clone();

        options.ServerListUri = serverListUri;

        return options;
    }

    public LauncherProcessOptions WithServers(params LauncherServerInfo[] servers)
    {
        return WithServers(servers.AsEnumerable());
    }

    public LauncherProcessOptions WithServers(IEnumerable<LauncherServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var builder = Clone();

        builder.Servers = servers.ToImmutableSortedDictionary(srv => srv.Id, srv => srv);

        return builder;
    }

    public LauncherProcessOptions AddServer(LauncherServerInfo server)
    {
        Check.Null(server);

        var builder = Clone();

        builder.Servers = Servers.Add(server.Id, server);

        return builder;
    }

    public LauncherProcessOptions AddServers(params LauncherServerInfo[] servers)
    {
        return AddServers(servers.AsEnumerable());
    }

    public LauncherProcessOptions AddServers(IEnumerable<LauncherServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var builder = Clone();

        builder.Servers = Servers.AddRange(servers.Select(srv => KeyValuePair.Create(srv.Id, srv)));

        return builder;
    }

    public LauncherProcessOptions RemoveServer(int id)
    {
        var builder = Clone();

        builder.Servers = Servers.Remove(id);

        return builder;
    }

    public LauncherProcessOptions RemoveServers(params int[] ids)
    {
        return RemoveServers(ids.AsEnumerable());
    }

    public LauncherProcessOptions RemoveServers(IEnumerable<int> ids)
    {
        var builder = Clone();

        builder.Servers = Servers.RemoveRange(ids);

        return builder;
    }

    public LauncherProcessOptions ClearServers()
    {
        return WithServers();
    }

    public LauncherProcessOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public LauncherProcessOptions WithWebUriProvider(Func<int, ReadOnlyMemory<string>, Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
