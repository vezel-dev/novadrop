using Vezel.Novadrop.Commands;

AnsiConsole.Profile.Capabilities.Ansi = !Console.IsOutputRedirected && !Console.IsErrorRedirected;

var app = new CommandApp();

app.Configure(cfg =>
{
    _ = cfg
        .SetApplicationName("novadrop-dc")
        .PropagateExceptions();

    _ = cfg
        .AddCommand<PackCommand>("pack")
        .WithDescription("Pack the contents of a directory to a data center file.");
    _ = cfg
        .AddCommand<RepackCommand>("repack")
        .WithDescription("Repack the contents of a data center file.");
    _ = cfg
        .AddCommand<SchemaCommand>("schema")
        .WithDescription("Infer schemas from a data center file.");
    _ = cfg
        .AddCommand<UnpackCommand>("unpack")
        .WithDescription("Unpack the contents of a data center file to a directory.");
    _ = cfg
        .AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate the contents of a directory against the data center schemas.");
    _ = cfg
        .AddCommand<VerifyCommand>("verify")
        .WithDescription("Verify the format integrity of a data center file.");
});

return await app.RunAsync(args);
