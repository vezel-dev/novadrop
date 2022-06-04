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

    public async Task ToggleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);

                _initialized = true;
            }

            await (!_enabled ? ApplyAsync(cancellationToken) : RevertAsync(cancellationToken)).ConfigureAwait(false);

            _enabled = !_enabled;
        }
        catch (Win32Exception ex)
        {
            throw new GamePatchException("A Win32 API error ocurred.", ex);
        }
    }

    protected abstract Task InitializeAsync(CancellationToken cancellationToken);

    protected abstract Task ApplyAsync(CancellationToken cancellationToken);

    protected abstract Task RevertAsync(CancellationToken cancellationToken);
}
