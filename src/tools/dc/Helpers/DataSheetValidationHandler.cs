namespace Vezel.Novadrop.Helpers;

sealed class DataSheetValidationHandler : IDisposable
{
    public bool HasProblems => _problems.Count != 0;

    readonly List<(FileInfo File, int, int, XmlSeverityType, string)> _problems = new();

    readonly InvocationContext? _context;

    public DataSheetValidationHandler(InvocationContext? context)
    {
        _context = context;
    }

    public void Dispose()
    {
        if (_problems.Count == 0)
            return;

        if (_context != null)
            _context.ExitCode = 1;

        Console.WriteLine();

        foreach (var fileGroup in _problems.GroupBy(tup => tup.File.Name))
        {
            var shownProblems = fileGroup.Take(10).ToArray();

            Console.WriteLine($"{fileGroup.Key}:");

            foreach (var (_, line, col, severity, msg) in shownProblems)
            {
                var (type, color) = severity switch
                {
                    XmlSeverityType.Error => ('E', ConsoleColor.Red),
                    XmlSeverityType.Warning => ('W', ConsoleColor.Yellow),
                    _ => throw new UnreachableException(),
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"  [{type}] ({line},{col}): {msg}");
                Console.ResetColor();
            }

            var remainingProblems = fileGroup.Count() - shownProblems.Length;

            if (remainingProblems != 0)
                Console.WriteLine($"    ... {remainingProblems} more problem(s) ...");
        }

        Console.WriteLine();
    }

    public void HandleException(FileInfo file, XmlException exception)
    {
        lock (_problems)
            _problems.Add(
                (file, exception.LineNumber, exception.LinePosition, XmlSeverityType.Error, exception.Message));
    }

    public ValidationEventHandler GetEventHandlerFor(FileInfo file)
    {
        return (_, e) =>
        {
            var ex = e.Exception;

            lock (_problems)
                _problems.Add((file, ex.LineNumber, ex.LinePosition, e.Severity, e.Message));
        };
    }
}
