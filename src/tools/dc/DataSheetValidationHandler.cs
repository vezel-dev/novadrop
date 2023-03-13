namespace Vezel.Novadrop;

internal sealed class DataSheetValidationHandler
{
    public bool HasProblems => _problems.Count != 0;

    private readonly List<(FileInfo File, int, int, XmlSeverityType, string)> _problems = new();

    public void Print()
    {
        if (_problems.Count == 0)
            return;

        Log.WriteLine();

        foreach (var fileGroup in _problems.GroupBy(tup => tup.File.Name))
        {
            var shownProblems = fileGroup.Take(10).ToArray();

            Log.WriteLine($"{fileGroup.Key}:");

            foreach (var (_, line, col, severity, msg) in shownProblems)
            {
                var (type, color) = severity switch
                {
                    XmlSeverityType.Error => ('E', "red"),
                    XmlSeverityType.Warning => ('W', "yellow"),
                    _ => throw new UnreachableException(),
                };

                Log.MarkupLineInterpolated($"  [[[{color}]{type}[/]]] ([blue]{line}[/],[blue]{col}[/]): {msg}");
            }

            var remainingProblems = fileGroup.Count() - shownProblems.Length;

            if (remainingProblems != 0)
                Log.MarkupLineInterpolated($"    ... [darkorange]{remainingProblems}[/] more problem(s) ...");
        }

        Log.WriteLine();
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
