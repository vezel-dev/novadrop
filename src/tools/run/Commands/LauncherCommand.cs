namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class LauncherCommand : CancellableAsyncCommand<LauncherCommand.LauncherCommandSettings>
{
    public sealed class LauncherCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<executable>")]
        [Description("Tl.exe path")]
        public string Executable { get; }

        [CommandArgument(1, "<account-name>")]
        [Description("Account name")]
        public string AccountName { get; }

        [CommandArgument(2, "<session-ticket>")]
        [Description("Session ticket")]
        public string SessionTicket { get; }

        [CommandArgument(3, "<url>")]
        [Description("Server list URL")]
        public Uri ServerListUri { get; }

        [CommandOption("--server-id <id>")]
        [Description("Set preferred server ID")]
        public int ServerId { get; init; }

        public LauncherCommandSettings(string executable, string accountName, string sessionTicket, Uri serverListUri)
        {
            Executable = executable;
            AccountName = accountName;
            SessionTicket = sessionTicket;
            ServerListUri = serverListUri;
        }
    }

    protected override Task<int> ExecuteAsync(
        dynamic expando,
        LauncherCommandSettings settings,
        ProgressContext progress,
        CancellationToken cancellationToken)
    {
        Log.WriteLine($"Running launcher and connecting to [cyan]{settings.ServerListUri}[/]...");

        return progress.RunTaskAsync(
            "Connecting to arbiter server",
            6,
            increment =>
            {
                var opts = new LauncherProcessOptions(
                    settings.Executable, settings.AccountName, settings.SessionTicket, settings.ServerListUri);

                if (settings.ServerId is not 0 and var id)
                    opts = opts.WithLastServerId(id);

                var process = new LauncherProcess(opts);

                process.ServerListUriRequested += increment;

                process.AuthenticationInfoRequested += increment;

                process.GameEventOccurred += e =>
                {
                    if (e is GameEvent.EnteredIntroCinematic or
                        GameEvent.EnteredServerList or
                        GameEvent.EnteringLobby or
                        GameEvent.EnteredLobby)
                        increment();
                };

                return process.RunAsync(cancellationToken);
            });
    }
}
