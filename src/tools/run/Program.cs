using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        var root = new RootCommand("Run the TERA launcher or client from the command line.")
        {
            new ClientCommand(),
            new LauncherCommand(),
        };

        return root.InvokeAsync(args);
    }
}
