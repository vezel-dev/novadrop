using Vezel.Novadrop.Commands;

AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

var app = new CommandApp();

app.Configure(cfg =>
{
    _ = cfg
        .SetApplicationName("novadrop-run")
        .PropagateExceptions();

    _ = cfg
        .AddCommand<ClientCommand>("client")
        .WithDescription("Run the TERA client.");
    _ = cfg
        .AddCommand<LauncherCommand>("launcher")
        .WithDescription("Run the TERA launcher.");
});

return await app.RunAsync(args);
