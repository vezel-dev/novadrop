AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

var app = new CommandApp();

// TODO: Add commands.
app.Configure(cfg =>
    cfg
        .SetApplicationName("novadrop-gpk")
        .PropagateExceptions());

return await app.RunAsync(args);
