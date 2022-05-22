namespace Vezel.Novadrop.Client;

public sealed class ClientProcessOptions
{
    public string FileName { get; private set; }

    public string AccountName { get; private set; }

    public ReadOnlyMemory<byte> Ticket { get; private set; }

    public string? Language { get; private set; }

    public ClientProcessOptions(string fileName, string accountName, ReadOnlyMemory<byte> ticket)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(accountName);

        FileName = fileName;
        AccountName = accountName;
        Ticket = ticket;
    }

    ClientProcessOptions Clone()
    {
        return new(FileName, AccountName, Ticket)
        {
            Language = Language,
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

    public ClientProcessOptions WithTicket(ReadOnlyMemory<byte> ticket)
    {
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
}
