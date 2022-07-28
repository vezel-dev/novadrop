namespace Vezel.Novadrop.Diagnostics;

[SuppressMessage("", "CA1001")]
internal sealed class ChildProcess
{
    // This is a simplified version of the ChildProcess class from Cathode.

    public int Id { get; }

    public Task<int> Completion { get; }

    private readonly Process _process;

    private readonly TaskCompletionSource<int> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource _exited = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [SuppressMessage("", "CA1031")]
    public ChildProcess(string fileName, string[] arguments, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo(fileName);

        foreach (var arg in arguments)
            info.ArgumentList.Add(arg);

        _process = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true,
        };

        var ctr = default(CancellationTokenRegistration);

        _process.Exited += (_, _) =>
        {
            ctr.Dispose();

            _ = _completion.TrySetResult(_process.ExitCode);
            _exited.SetResult();
        };

        _ = _process.Start();

        Id = _process.Id;

        // We register the cancellation callback here, after it has started, so that we do not potentially kill the
        // process prior to or during startup.
        ctr = cancellationToken.UnsafeRegister(
            static (state, token) => ((ChildProcess)state!)._completion.TrySetCanceled(token), this);

        Completion = Task.Run(async () =>
        {
            try
            {
                return await _completion.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    _process.Kill(true);
                }
                catch (Exception)
                {
                    // Even if killing the process tree somehow fails, there is nothing we can do about it here.
                }

                // Normally, _completion is completed from the Exited event handler. In the case of cancellation, we
                // complete it from the cancellation callback. This means that we have to wait for the Exited event
                // handler to run so that it becomes safe to dispose the process.
                await _exited.Task.ConfigureAwait(false);

                throw;
            }
            finally
            {
                // At this point, we know we are completely finished with the process.
                _process.Dispose();
            }
        });
    }
}
