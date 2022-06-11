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
                .SetApplicationName("novadrop-rc")
                .PropagateExceptions();

            _ = cfg
                .AddCommand<PackCommand>("pack")
                .WithDescription("Pack the contents of a directory to a resource container file.");
            _ = cfg
                .AddCommand<RepackCommand>("repack")
                .WithDescription("Repack the contents of a resource container file.");
            _ = cfg
                .AddCommand<UnpackCommand>("unpack")
                .WithDescription("Unpack the contents of a resource container file to a directory.");
            _ = cfg
                .AddCommand<VerifyCommand>("verify")
                .WithDescription("Verify the format integrity of a resource container file.");
        });

        return app.RunAsync(args);
    }
}
