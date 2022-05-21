using Vezel.Novadrop.Helpers;

namespace Vezel.Novadrop.Commands;

sealed class ValidateCommand : Command
{
    public ValidateCommand()
        : base("validate", "Validate the contents of a directory against the data center schemas.")
    {
        var inputArg = new Argument<DirectoryInfo>(
            "input",
            "Input directory")
            .ExistingOnly();

        Add(inputArg);

        this.SetHandler(
            async (
                InvocationContext context,
                DirectoryInfo input,
                CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Validating sheets in '{input}'...");

                var sw = Stopwatch.StartNew();

                var files = input.GetFiles("?*-?*.xml", SearchOption.AllDirectories);

                using (var handler = new DataSheetValidationHandler(context))
                    await Parallel.ForEachAsync(
                        files,
                        cancellationToken,
                        async (file, cancellationToken) =>
                        {
                            var settings = new XmlReaderSettings
                            {
                                XmlResolver = new XmlUrlResolver(),
                                ValidationType = ValidationType.Schema,
                                ValidationFlags =
                                    XmlSchemaValidationFlags.ProcessSchemaLocation |
                                    XmlSchemaValidationFlags.ReportValidationWarnings,
                                Async = true,
                            };

                            settings.ValidationEventHandler += handler.GetEventHandlerFor(file);

                            using var reader = XmlReader.Create(file.FullName, settings);

                            while (await reader.ReadAsync())
                            {
                            }
                        });

                sw.Stop();

                Console.WriteLine($"Validated {files.Length} data sheets in {sw.Elapsed}.");
            },
            inputArg);
    }
}
