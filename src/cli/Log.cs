namespace Vezel.Novadrop.Cli;

internal static class Log
{
    public static void WriteLine()
    {
        AnsiConsole.WriteLine();
    }

    public static void WriteLine(string value)
    {
        AnsiConsole.WriteLine(value);
    }

    public static void MarkupLine(string value)
    {
        AnsiConsole.MarkupLine(CultureInfo.CurrentCulture, value);
    }

    public static void MarkupLineInterpolated(FormattableString value)
    {
        AnsiConsole.MarkupLineInterpolated(CultureInfo.CurrentCulture, value);
    }
}
