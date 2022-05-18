using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        var root = new RootCommand("Manipulate TERA's resource container files.")
        {
            new PackCommand(),
            new UnpackCommand(),
            new RepackCommand(),
            new VerifyCommand(),
        };

        return root.InvokeAsync(args);
    }
}
