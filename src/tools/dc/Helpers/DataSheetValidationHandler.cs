namespace Vezel.Novadrop.Helpers;

sealed class DataSheetValidationHandler : IDisposable
{
    readonly List<(FileInfo File, ValidationEventArgs Args)> _problems = new();

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

            foreach (var problem in shownProblems)
            {
                var ex = problem.Args.Exception;
                var (msg, color) = problem.Args.Severity switch
                {
                    XmlSeverityType.Error => ('E', ConsoleColor.Red),
                    XmlSeverityType.Warning => ('W', ConsoleColor.Yellow),
                    _ => throw new UnreachableException(),
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"  [{msg}] ({ex.LineNumber},{ex.LinePosition}): {ex.Message}");
                Console.ResetColor();
            }

            var remainingProblems = fileGroup.Count() - shownProblems.Length;

            if (remainingProblems != 0)
                Console.WriteLine($"    ... {remainingProblems} more problem(s) ...");
        }

        Console.WriteLine();
    }

    public ValidationEventHandler GetEventHandlerFor(FileInfo file)
    {
        return (_, e) =>
        {
            lock (_problems)
                _problems.Add((file, e));
        };
    }
}
