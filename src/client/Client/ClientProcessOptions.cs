namespace Vezel.Novadrop.Client;

public sealed class ClientProcessOptions
{
    public string FileName { get; private set; } = null!;

    public string AccountName { get; private set; } = null!;

    public string Ticket { get; private set; } = null!;

    public string? Language { get; private set; }

    public IReadOnlyDictionary<int, ClientServerInfo> Servers { get; private set; } = null!;

    public int LastServerId { get; private set; }

    ClientProcessOptions()
    {
    }

    public ClientProcessOptions(
        string fileName, string accountName, string ticket, IEnumerable<ClientServerInfo> servers)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(accountName);
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(servers);
        _ = servers.Any() ? true : throw new ArgumentException(null, nameof(servers));
        _ = servers.All(s => s != null) ? true : throw new ArgumentException(null, nameof(servers));

        FileName = fileName;
        AccountName = accountName;
        Ticket = ticket;
        Servers = servers.ToImmutableDictionary(s => s.Id);
    }

    ClientProcessOptions Clone()
    {
        return new()
        {
            FileName = FileName,
            AccountName = AccountName,
            Ticket = Ticket,
            Language = Language,
            Servers = Servers,
            LastServerId = LastServerId,
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

    public ClientProcessOptions WithTicket(string ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        var options = Clone();

        options.Ticket = ticket;

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
}
