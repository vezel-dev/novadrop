// SPDX-License-Identifier: 0BSD

namespace Vezel.Novadrop.Cli;

internal abstract class CancellableAsyncCommand<TSettings> : AsyncCommand<TSettings>
    where TSettings : CommandSettings
{
    public override sealed async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        var expando = new ExpandoObject();

        await PreExecuteAsync(expando, settings, cancellationToken).ConfigureAwait(false);

        var code = await AnsiConsole.Progress()
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
            .StartAsync(ctx => ExecuteAsync(expando, settings, ctx, cancellationToken))
            .ConfigureAwait(false);

        await PostExecuteAsync(expando, settings, cancellationToken).ConfigureAwait(false);

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
