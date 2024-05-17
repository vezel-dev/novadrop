// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop;

internal sealed class DataSheetValidationHandler
{
    private record struct Diagnostic(FileInfo File, int Line, int Column, XmlSeverityType Severity, string Message);

    public bool HasDiagnostics => !_diagnostics.IsEmpty;

    private readonly ConcurrentBag<Diagnostic> _diagnostics = [];

    public void Print()
    {
        foreach (var diag in _diagnostics
            .OrderBy(static diag => diag.File.FullName)
            .ThenBy(static diag => diag.Line)
            .ThenBy(static diag => diag.Column)
            .ThenBy(static diag => diag.Severity))
        {
            var color = diag.Severity switch
            {
                XmlSeverityType.Error => "red",
                XmlSeverityType.Warning => "yellow",
                _ => throw new UnreachableException(),
            };

            Log.MarkupLineInterpolated(
                $"[{color}]{diag.File}({diag.Line},{diag.Column}): {diag.Severity}: {diag.Message}[/]");
        }
    }

    public void HandleException(FileInfo file, XmlException exception)
    {
        _diagnostics.Add(
            new(file, exception.LineNumber, exception.LinePosition, XmlSeverityType.Error, exception.Message));
    }

    public ValidationEventHandler GetEventHandlerFor(FileInfo file)
    {
        return (_, e) =>
        {
            var ex = e.Exception;

            _diagnostics.Add(new(file, ex.LineNumber, ex.LinePosition, e.Severity, ex.Message));
        };
    }
}
