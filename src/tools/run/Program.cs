using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
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

        return app.RunAsync(args);
    }
}
