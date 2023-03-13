namespace Vezel.Novadrop.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class ValidateCommand : CancellableAsyncCommand<ValidateCommand.ValidateCommandSettings>
{
    public sealed class ValidateCommandSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input directory")]
        public string Input { get; }

        public ValidateCommandSettings(string input)
        {
            Input = input;
        }
    }

    protected override Task PreExecuteAsync(
        dynamic expando, ValidateCommandSettings settings, CancellationToken cancellationToken)
    {
        expando.Handler = new DataSheetValidationHandler();

        return Task.CompletedTask;
    }

    protected override async Task<int> ExecuteAsync(
        dynamic expando,
        ValidateCommandSettings settings,
        ProgressContext progress,
        CancellationToken cancellationToken)
    {
        Log.MarkupLineInterpolated($"Validating data sheets in [cyan]{settings.Input}[/]...");

        var files = await progress.RunTaskAsync(
            "Gather data sheet files",
            () => Task.FromResult(
                new DirectoryInfo(settings.Input)
                    .EnumerateFiles("?*-?*.xml", SearchOption.AllDirectories)
                    .OrderBy(f => f.FullName, StringComparer.Ordinal)
                    .ToArray()));

        var handler = (DataSheetValidationHandler)expando.Handler;

        await progress.RunTaskAsync(
            "Validate data sheets",
            files.Length,
            increment => Parallel.ForEachAsync(
                files,
                cancellationToken,
                async (file, cancellationToken) =>
                {
                    var xmlSettings = new XmlReaderSettings
                    {
                        XmlResolver = new XmlUrlResolver(),
                        ValidationType = ValidationType.Schema,
                        ValidationFlags =
                            XmlSchemaValidationFlags.ProcessSchemaLocation |
                            XmlSchemaValidationFlags.ReportValidationWarnings,
                        Async = true,
                    };

                    xmlSettings.ValidationEventHandler += handler.GetEventHandlerFor(file);

                    using var reader = XmlReader.Create(file.FullName, xmlSettings);

                    try
                    {
                        while (await reader.ReadAsync())
                        {
                        }
                    }
                    catch (XmlException ex)
                    {
                        handler.HandleException(file, ex);
                    }

                    increment();
                }));

        return handler.HasProblems ? 1 : 0;
    }

    protected override Task PostExecuteAsync(
        dynamic expando, ValidateCommandSettings settings, CancellationToken cancellationToken)
    {
        expando.Handler.Print();

        return Task.CompletedTask;
    }
}
