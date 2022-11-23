namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

        // TODO: Add commands.
        var app = new CommandApp();

        app.Configure(cfg =>
            cfg
                .SetApplicationName("novadrop-gpk")
                .PropagateExceptions());

        return app.RunAsync(args);
    }
}
