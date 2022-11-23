using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

        var app = new CommandApp<ScanCommand>();

        app.Configure(cfg =>
            cfg
                .SetApplicationName("novadrop-scan")
                .PropagateExceptions());

        return app.RunAsync(args);
    }
}
