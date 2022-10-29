namespace Vezel.Novadrop.Client;

public sealed class LauncherProcessOptions
{
    public string FileName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string SessionTicket { get; private set; } = null!;

    public Uri ServerListUri { get; private set; } = null!;

    public IReadOnlyDictionary<int, LauncherServerInfo> Servers { get; private set; } =
        new Dictionary<int, LauncherServerInfo>();

    public int LastServerId { get; private set; }

    public Func<int, string[], Uri>? WebUriProvider { get; private set; }

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

    [SuppressMessage("", "CA1851")]
    public LauncherProcessOptions WithServers(IEnumerable<LauncherServerInfo> servers)
    {
        Check.Null(servers);
        Check.All(servers, static srv => srv != null);

        var options = Clone();

        options.Servers = servers.ToImmutableDictionary(s => s.Id);

        return options;
    }

    public LauncherProcessOptions WithLastServerId(int lastServerId)
    {
        Check.Range(lastServerId > 0, lastServerId);

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public LauncherProcessOptions WithWebUriProvider(Func<int, string[], Uri>? webUriProvider)
    {
        Check.Null(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
