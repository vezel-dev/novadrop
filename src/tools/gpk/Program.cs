// TODO: Add commands.

var app = new CommandApp();

app.Configure(cfg =>
    cfg
        .SetApplicationName("novadrop-gpk")
        .PropagateExceptions());

return await app.RunAsync(args);
