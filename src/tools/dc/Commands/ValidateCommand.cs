namespace Vezel.Novadrop.Commands;

sealed class ValidateCommand : Command
{
    public ValidateCommand()
        : base("validate", "Validate the contents of a directory against the data center schemas.")
    {
        var inputArg = new Argument<DirectoryInfo>("input", "Input directory");

        Add(inputArg);

        this.SetHandler(
            async (InvocationContext context, DirectoryInfo input, CancellationToken cancellationToken) =>
            {
                Console.WriteLine($"Validating sheets in '{input}'...");

                var sw = Stopwatch.StartNew();

                var files = input.EnumerateFiles("?*-?*.xml", SearchOption.AllDirectories).ToArray();
                var problems = new List<(string Name, FileInfo File, ValidationEventArgs Args)>();

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

                        settings.ValidationEventHandler += (_, e) =>
                        {
                            lock (problems)
                                problems.Add((file.Directory!.Name, file, e));
                        };

                        using var reader = XmlReader.Create(file.FullName, settings);

                        while (await reader.ReadAsync())
                        {
                        }
                    });

                var count = problems.Select(tup => tup.File).Distinct();

                if (problems.Count != 0)
                {
                    foreach (var nameGroup in problems.GroupBy(tup => tup.File.Directory!.Name))
                    {
                        Console.WriteLine($"{nameGroup.Key}/");

                        foreach (var fileGroup in nameGroup.GroupBy(tup => tup.File.Name))
                        {
                            var shownProblems = fileGroup.Take(10).ToArray();

                            Console.WriteLine($"  {fileGroup.Key}:");

                            foreach (var problem in shownProblems)
                            {
                                var ex = problem.Args.Exception;
                                var (msg, color) = problem.Args.Severity switch
                                {
                                    XmlSeverityType.Error => ('E', ConsoleColor.Red),
                                    XmlSeverityType.Warning => ('W', ConsoleColor.Yellow),
                                    _ => throw new InvalidOperationException(), // Impossible.
                                };

                                Console.ForegroundColor = color;
                                Console.WriteLine($"    [{msg}] ({ex.LineNumber},{ex.LinePosition}): {ex.Message}");
                                Console.ResetColor();
                            }

                            var remainingProblems = fileGroup.Count() - shownProblems.Length;

                            if (remainingProblems != 0)
                                Console.WriteLine($"    ... {remainingProblems} more problems ...");
                        }

                        context.ExitCode = 1;
                    }

                    Console.WriteLine();
                }

                sw.Stop();

                Console.WriteLine($"{count}/{files.Length} data sheets validated in {sw.Elapsed}.");
            },
            inputArg);
    }
}
