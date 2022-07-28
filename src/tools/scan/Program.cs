using Vezel.Novadrop.Commands;

var app = new CommandApp<ScanCommand>();

app.Configure(cfg =>
    cfg
        .SetApplicationName("novadrop-scan")
        .PropagateExceptions());

return await app.RunAsync(args);
