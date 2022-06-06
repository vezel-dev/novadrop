namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
sealed class ClientCommand : CancellableAsyncCommand<ClientCommand.ClientCommandSettings>
{
    public sealed class ClientCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<executable>")]
        [Description("TERA.exe path")]
        public string Executable { get; }

        [CommandArgument(1, "<language>")]
        [Description("Data center language")]
        public string Language { get; }

        [CommandArgument(2, "<account-name>")]
        [Description("Account name")]
        public string AccountName { get; }

        [CommandArgument(3, "<session-ticket>")]
        [Description("Session ticket")]
        public string SessionTicket { get; }

        [CommandArgument(4, "<server-host>")]
        [Description("Server host")]
        public string ServerHost { get; }

        [CommandArgument(5, "<server-port>")]
        [Description("Server port")]
        public ushort ServerPort { get; }

        [CommandOption("--patch")]
        [Description("Enable Themida and telemetry removal")]
        public bool Patch { get; init; }

        public ClientCommandSettings(
            string executable,
            string language,
            string accountName,
            string sessionTicket,
            string serverHost,
            ushort serverPort)
        {
            Executable = executable;
            Language = language;
            AccountName = accountName;
            SessionTicket = sessionTicket;
            ServerHost = serverHost;
            ServerPort = serverPort;
        }
    }

    protected override Task<int> ExecuteAsync(
        dynamic expando, ClientCommandSettings settings, ProgressContext progress, CancellationToken cancellationToken)
    {
        var srvName = $"{settings.ServerHost}:{settings.ServerPort}";
        var srv = new ClientServerInfo(
            42,
            string.Empty,
            srvName,
            srvName,
            string.Empty,
            string.Empty,
            true,
            string.Empty,
            settings.ServerHost,
            null,
            settings.ServerPort);

        Log.WriteLine($"Running client and connecting to [cyan]{srvName}[/]...");

        return progress.RunTaskAsync(
            "Connecting to arbiter server",
            8 + (settings.Patch ? 2 : 0),
            increment =>
            {
                var process = new ClientProcess(
                    new ClientProcessOptions(
                        settings.Executable, settings.AccountName, settings.SessionTicket, new[] { srv })
                        .WithLanguage(settings.Language)
                        .WithLastServerId(42));

                var patches = new List<(GamePatch, Task)>();

                process.GameStarted += _ =>
                {
                    increment();

                    if (!settings.Patch)
                        return;

                    var nativeProc = new NativeProcess(process.Id);
                    var tempPatches = new GamePatch[]
                    {
                        new SecurityNeutralizationPatch(nativeProc),
                        new TelemetryRemovalPatch(nativeProc),
                    };

                    // Start scanning for needed patterns as early as possible.
                    foreach (var patch in tempPatches)
                        patches.Add((patch, patch.InitializeAsync(cancellationToken)));
                };

                var sls = false;

                process.ServerListRequested += () =>
                {
                    // If we take too long to apply patches, the client gets impatient and sends the request again.
                    if (sls)
                        return;

                    sls = true;

                    increment();

                    foreach (var (patch, task) in patches)
                    {
                        task.GetAwaiter().GetResult();
                        patch.Toggle();

                        increment();
                    }
                };

                process.AccountNameRequested += increment;

                process.SessionTicketRequested += increment;

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
