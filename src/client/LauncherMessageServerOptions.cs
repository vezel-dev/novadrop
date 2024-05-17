// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Client;

public sealed class LauncherMessageServerOptions
{
    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public Uri ServerListUri { get; private set; } = null!;

    public ImmutableSortedDictionary<int, LauncherServerInfo> Servers { get; private set; } =
        ImmutableSortedDictionary<int, LauncherServerInfo>.Empty;

    public int LastServerId { get; private set; }

    public Func<int, ReadOnlyMemory<string>, Uri>? WebUriProvider { get; private set; }

    private LauncherMessageServerOptions()
    {
    }

    public LauncherMessageServerOptions(string accountName, string sessionTicket, Uri serverListUri)
    {
        Check.Null(accountName);
        Check.Null(sessionTicket);
        Check.Null(serverListUri);
        Check.Argument(serverListUri.IsAbsoluteUri, serverListUri);

        AccountName = accountName;
        SessionTicket = sessionTicket;
        ServerListUri = serverListUri;
    }

    private LauncherMessageServerOptions Clone()
    {
        return new()
        {
            AccountName = AccountName,
            SessionTicket = SessionTicket,
            ServerListUri = ServerListUri,
            Servers = Servers,
            LastServerId = LastServerId,
            WebUriProvider = WebUriProvider,
        };
    }

    public LauncherMessageServerOptions WithAccountName(string accountName)
    {
        Check.Null(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public LauncherMessageServerOptions WithSessionTicket(string sessionTicket)
    {
        Check.Null(sessionTicket);

        var options = Clone();

        options.SessionTicket = sessionTicket;

        return options;
    }

    public LauncherMessageServerOptions WithServerListUri(Uri serverListUri)
    {
        Check.Null(serverListUri);
        Check.Argument(serverListUri.IsAbsoluteUri, serverListUri);

        var options = Clone();

        options.ServerListUri = serverListUri;

        return options;
    }

    public LauncherMessageServerOptions WithServers(params LauncherServerInfo[] servers)
    {
        return WithServers(servers.AsEnumerable());
    }

    public LauncherMessageServerOptions WithServers(IEnumerable<LauncherServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = servers.ToImmutableSortedDictionary(static srv => srv.Id, static srv => srv);

        return options;
    }

    public LauncherMessageServerOptions AddServer(LauncherServerInfo server)
    {
        Check.Null(server);

        var options = Clone();

        options.Servers = Servers.Add(server.Id, server);

        return options;
    }

    public LauncherMessageServerOptions AddServers(params LauncherServerInfo[] servers)
    {
        return AddServers(servers.AsEnumerable());
    }

    public LauncherMessageServerOptions AddServers(IEnumerable<LauncherServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = Servers.AddRange(servers.Select(static srv => KeyValuePair.Create(srv.Id, srv)));

        return options;
    }

    public LauncherMessageServerOptions RemoveServer(int id)
    {
        var options = Clone();

        options.Servers = Servers.Remove(id);

        return options;
    }

    public LauncherMessageServerOptions RemoveServers(params int[] ids)
    {
        return RemoveServers(ids.AsEnumerable());
    }

    public LauncherMessageServerOptions RemoveServers(IEnumerable<int> ids)
    {
        var options = Clone();

        options.Servers = Servers.RemoveRange(ids);

        return options;
    }

    public LauncherMessageServerOptions ClearServers()
    {
        return WithServers();
    }

    public LauncherMessageServerOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public LauncherMessageServerOptions WithWebUriProvider(Func<int, ReadOnlyMemory<string>, Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
