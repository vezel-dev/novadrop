namespace Vezel.Novadrop.Commands;

sealed class ClientCommand : Command
{
    public ClientCommand()
        : base("client", "Run the TERA client.")
    {
        var executableArg = new Argument<FileInfo>(
            "executable",
            "TERA.exe path")
            .ExistingOnly();
        var languageArg = new Argument<string>(
            "language",
            "Data center language");
        var accountArg = new Argument<string>(
            "account",
            "Account name");
        var ticketArg = new Argument<string>(
            "ticket",
            "Authentication ticket");
        var serverHostArg = new Argument<string>(
            "server-host",
            "Server host");
        var serverPortArg = new Argument<ushort>(
            "server-port",
            "Server port");

        Add(executableArg);
        Add(languageArg);
        Add(accountArg);
        Add(ticketArg);
        Add(serverHostArg);
        Add(serverPortArg);

        this.SetHandler(
            async (
                InvocationContext context,
                FileInfo executable,
                string language,
                string account,
                string ticket,
                string serverHost,
                ushort serverPort,
                CancellationToken cancellationToken) =>
            {
                var ip = await Dns.GetHostAddressesAsync(serverHost, AddressFamily.InterNetwork, cancellationToken);

                if (ip.Length == 0)
                    throw new ApplicationException($"Could not resolve server host '{serverHost}'.");

                var server = new ServerInfo(
                    42,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true,
                    string.Empty,
                    ip[0],
                    serverPort);

                Console.WriteLine("Running client and connecting to '{0}'...", server.Endpoint);

                context.ExitCode = await new ClientProcess(
                    new ClientProcessOptions(executable.FullName, account, ticket, new[] { server })
                        .WithLanguage(language)
                        .WithLastServerId(42))
                    .RunAsync(cancellationToken);
            },
            executableArg,
            languageArg,
            accountArg,
            ticketArg,
            serverHostArg,
            serverPortArg);
    }
}
