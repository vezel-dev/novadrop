using Vezel.Novadrop.Memory;

namespace Vezel.Novadrop.Patches;

public abstract class GamePatch
{
    public NativeProcess Process { get; }

    public bool Active { get; private set; }

    protected MemoryWindow Window => Process.MainModule.Window;

    bool _initialized;

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

            await (!Active ? ApplyAsync(cancellationToken) : RevertAsync(cancellationToken)).ConfigureAwait(false);

            Active = !Active;
        }
        catch (Win32Exception ex)
        {
            throw new GamePatchException("A Win32 API error ocurred.", ex);
        }
    }

    protected abstract Task InitializeAsync(CancellationToken cancellationToken);

    protected abstract Task ApplyAsync(CancellationToken cancellationToken);

    protected abstract Task RevertAsync(CancellationToken cancellationToken);

    public override string ToString()
    {
        return $"{{Name: {GetType().Name}, Process: {Process}, Active: {Active}}}";
    }
}
