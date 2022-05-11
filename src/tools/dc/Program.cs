using Vezel.Novadrop.Commands;

namespace Vezel.Novadrop;

static class Program
{
    static Task<int> Main(string[] args)
    {
        var root = new RootCommand("Manipulate TERA's data center files.")
        {
            new PackCommand(),
            new UnpackCommand(),
            new RepackCommand(),
            new ValidateCommand(),
            new VerifyCommand(),
        };

        return root.InvokeAsync(args);
    }
}
