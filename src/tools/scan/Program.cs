using Vezel.Novadrop.Commands;

AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

var app = new CommandApp<ScanCommand>();

app.Configure(cfg =>
    cfg
        .SetApplicationName("novadrop-scan")
        .PropagateExceptions());

return await app.RunAsync(args);
