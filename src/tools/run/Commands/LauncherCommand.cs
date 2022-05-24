namespace Vezel.Novadrop.Commands;

sealed class LauncherCommand : Command
{
    public LauncherCommand()
        : base("launcher", "Run the TERA launcher.")
    {
        var executableArg = new Argument<FileInfo>(
            "executable",
            "Tl.exe path")
            .ExistingOnly();
        var accountArg = new Argument<string>(
            "account",
            "Account name");
        var ticketArg = new Argument<string>(
            "ticket",
            "Authentication ticket");
        var urlArg = new Argument<Uri>(
            "url",
            "Server list URL");
        var serverIdOpt = new Option<int>(
            "--server-id",
            () => 0,
            "Preferred server ID");

        Add(executableArg);
        Add(accountArg);
        Add(ticketArg);
        Add(urlArg);
        Add(serverIdOpt);

        this.SetHandler(
            async (
                InvocationContext context,
                FileInfo executable,
                string account,
                string ticket,
                Uri url,
                int serverId,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine("Running launcher and connecting to '{0}'...", url);

                var opts = new LauncherProcessOptions(executable.FullName, account, ticket, url);

                if (serverId != 0)
                    opts = opts.WithLastServerId(serverId);

                context.ExitCode = await new LauncherProcess(opts).RunAsync(cancellationToken);
            },
            executableArg,
            accountArg,
            ticketArg,
            urlArg,
            serverIdOpt);
    }
}
