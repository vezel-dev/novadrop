namespace Vezel.Novadrop.Cli;

static class CliExtensions
{
    sealed class ProgressTaskWrapper : IDisposable
    {
        readonly ProgressTask _task;

        public ProgressTaskWrapper(ProgressContext context, string description, int goal, bool indeterminate)
        {
            _task = context.AddTask(description, maxValue: goal);

            if (indeterminate)
                _ = _task.IsIndeterminate();

            _task.StartTask();
        }

        public void Increment()
        {
            _task.Increment(1);
        }

        public void Dispose()
        {
            _task.StopTask();
        }
    }

    public static async Task<T> RunTaskAsync<T>(
        this ProgressContext context, string description, Func<Task<T>> function)
    {
        using var task = new ProgressTaskWrapper(context, description, 1, true);

        var result = await function().ConfigureAwait(false);

        task.Increment();

        return result;
    }

    public static async Task RunTaskAsync(
        this ProgressContext context, string description, Func<Task> function)
    {
        _ = await context.RunTaskAsync(description, async () =>
        {
            await function().ConfigureAwait(false);

            return 0;
        }).ConfigureAwait(false);
    }

    public static async Task<T> RunTaskAsync<T>(
        this ProgressContext context, string description, int goal, Func<Action, Task<T>> function)
    {
        using var task = new ProgressTaskWrapper(context, description, goal, false);

        return await function(() => task.Increment()).ConfigureAwait(false);
    }

    public static async Task RunTaskAsync(
        this ProgressContext context, string description, int goal, Func<Action, Task> function)
    {
        _ = await context.RunTaskAsync(description, goal, async increment =>
        {
            await function(increment).ConfigureAwait(false);

            return 0;
        }).ConfigureAwait(false);
    }
}
