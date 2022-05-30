namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        // TODO: Add commands.

        var app = new CommandApp();

        app.Configure(cfg =>
            cfg
                .SetApplicationName("novadrop-gpk")
                .PropagateExceptions());

        return app.RunAsync(args);
    }
}
