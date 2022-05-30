using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        var app = new CommandApp<ScanCommand>();

        app.Configure(cfg =>
            cfg
                .SetApplicationName("novadrop-scan")
                .PropagateExceptions());

        return app.RunAsync(args);
    }
}
