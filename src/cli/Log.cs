namespace Vezel.Novadrop.Cli;

internal static class Log
{
    public static void WriteLine()
    {
        AnsiConsole.WriteLine();
    }

    public static void WriteLine(FormattableString value)
    {
        AnsiConsole.MarkupLineInterpolated(CultureInfo.CurrentCulture, value);
    }
}
