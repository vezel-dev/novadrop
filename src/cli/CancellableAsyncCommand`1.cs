namespace Vezel.Novadrop.Cli;

internal abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
    where TSettings : CommandSettings
{
    public override sealed async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        using var cts = new CancellationTokenSource();

        void HandleSignal(PosixSignalContext context)
        {
            context.Cancel = true;

            cts.Cancel();
        }

        using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
        using var sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal);
        using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);

        var expando = new ExpandoObject();
        var token = cts.Token;

        await PreExecuteAsync(expando, settings, token).ConfigureAwait(false);

        int code;

        try
        {
            code = await AnsiConsole.Progress()
                .Columns(
                    new ProgressBarColumn
                    {
                        Width = 60,
                    },
                    new PercentageColumn
                    {
                        Style = new(Color.Yellow),
                    },
                    new ElapsedTimeColumn(),
                    new TaskDescriptionColumn
                    {
                        Alignment = Justify.Left,
                    })
                .StartAsync(ctx => ExecuteAsync(expando, settings, ctx, token))
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return 1;
        }

        await PostExecuteAsync(expando, settings, token).ConfigureAwait(false);

        return code;
    }

    protected virtual Task PreExecuteAsync(dynamic expando, TSettings settings, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected abstract Task<int> ExecuteAsync(
        dynamic expando, TSettings settings, ProgressContext progress, CancellationToken cancellationToken);

    protected virtual Task PostExecuteAsync(dynamic expando, TSettings settings, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
