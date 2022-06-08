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

    LauncherProcessOptions()
    {
    }

    public LauncherProcessOptions(string fileName, string accountName, string sessionTicket, Uri serverListUri)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentNullException.ThrowIfNull(sessionTicket);
        ArgumentNullException.ThrowIfNull(serverListUri);
        _ = serverListUri.IsAbsoluteUri ? true : throw new ArgumentException(null, nameof(serverListUri));

        FileName = fileName;
        AccountName = accountName;
        SessionTicket = sessionTicket;
        ServerListUri = serverListUri;
    }

    LauncherProcessOptions Clone()
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
        ArgumentNullException.ThrowIfNull(fileName);

        var options = Clone();

        options.FileName = fileName;

        return options;
    }

    public LauncherProcessOptions WithAccountName(string accountName)
    {
        ArgumentNullException.ThrowIfNull(accountName);

        var options = Clone();

        options.AccountName = accountName;

        return options;
    }

    public LauncherProcessOptions WithSessionTicket(string sessionTicket)
    {
        ArgumentNullException.ThrowIfNull(sessionTicket);

        var options = Clone();

        options.SessionTicket = sessionTicket;

        return options;
    }

    public LauncherProcessOptions WithServerListUri(Uri serverListUri)
    {
        ArgumentNullException.ThrowIfNull(serverListUri);
        _ = serverListUri.IsAbsoluteUri ? true : throw new ArgumentException(null, nameof(serverListUri));

        var options = Clone();

        options.ServerListUri = serverListUri;

        return options;
    }

    public LauncherProcessOptions WithServers(IEnumerable<LauncherServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(servers);
        _ = servers.All(s => s != null) ? true : throw new ArgumentException(null, nameof(servers));

        var options = Clone();

        options.Servers = servers.ToImmutableDictionary(s => s.Id);

        return options;
    }

    public LauncherProcessOptions WithLastServerId(int lastServerId)
    {
        _ = lastServerId > 0 ? true : throw new ArgumentOutOfRangeException(nameof(lastServerId));

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }

    public LauncherProcessOptions WithWebUriProvider(Func<int, string[], Uri>? webUriProvider)
    {
        ArgumentNullException.ThrowIfNull(webUriProvider);

        var options = Clone();

        options.WebUriProvider = webUriProvider;

        return options;
    }
}
