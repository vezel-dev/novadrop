namespace Vezel.Novadrop.Client;

public sealed class LauncherProcessOptions
{
    public string FileName { get; private set; }

    public string AccountName { get; private set; }

    public ReadOnlyMemory<byte> Ticket { get; private set; }

    public Uri ServerListUri { get; private set; }

    public int LastServerId { get; private set; }

    public LauncherProcessOptions(string fileName, string accountName, ReadOnlyMemory<byte> ticket, Uri serverListUri)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentNullException.ThrowIfNull(serverListUri);

        FileName = fileName;
        AccountName = accountName;
        Ticket = ticket;
        ServerListUri = serverListUri;
    }

    LauncherProcessOptions Clone()
    {
        return new(FileName, AccountName, Ticket, ServerListUri)
        {
            LastServerId = LastServerId,
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

    public LauncherProcessOptions WithTicket(ReadOnlyMemory<byte> ticket)
    {
        var options = Clone();

        options.Ticket = ticket;

        return options;
    }

    public LauncherProcessOptions WithServerListUri(Uri serverListUri)
    {
        ArgumentNullException.ThrowIfNull(serverListUri);

        var options = Clone();

        options.ServerListUri = serverListUri;

        return options;
    }

    public LauncherProcessOptions WithLastServerId(int lastServerId)
    {
        _ = lastServerId > 0 ? true : throw new ArgumentOutOfRangeException(nameof(lastServerId));

        var options = Clone();

        options.LastServerId = lastServerId;

        return options;
    }
}
