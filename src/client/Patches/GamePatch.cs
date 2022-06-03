using Vezel.Novadrop.Memory;

namespace Vezel.Novadrop.Patches;

public abstract class GamePatch
{
    public NativeProcess Process { get; }

    protected MemoryWindow Executable => Process.MainModule;

    bool _initialized;

    bool _enabled;

    private protected GamePatch(NativeProcess process)
    {
        ArgumentNullException.ThrowIfNull(process);

        Process = process;
    }

    public async Task<bool> ToggleAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            if (!await InitializeAsync(cancellationToken).ConfigureAwait(false))
                return false;

            _initialized = true;
        }

        var task = !_enabled ? ApplyAsync(cancellationToken) : RevertAsync(cancellationToken);

        if (!await task.ConfigureAwait(false))
            return false;

        _enabled = !_enabled;

        return true;
    }

    protected abstract Task<bool> InitializeAsync(CancellationToken cancellationToken);

    protected abstract Task<bool> ApplyAsync(CancellationToken cancellationToken);

    protected abstract Task<bool> RevertAsync(CancellationToken cancellationToken);
}
