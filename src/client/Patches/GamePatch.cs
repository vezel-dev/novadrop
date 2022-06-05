using Vezel.Novadrop.Memory;

namespace Vezel.Novadrop.Patches;

public abstract class GamePatch
{
    public NativeProcess Process { get; }

    public bool IsActive { get; private set; }

    protected MemoryWindow Window { get; }

    bool _initialized;

    private protected GamePatch(NativeProcess process)
    {
        ArgumentNullException.ThrowIfNull(process);

        Process = process;
        Window = process.MainModule.Window;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        try
        {
            await InitializeCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Win32Exception ex)
        {
            throw new GamePatchException("A Win32 API error ocurred.", ex);
        }

        _initialized = true;
    }

    protected abstract Task InitializeCoreAsync(CancellationToken cancellationToken);

    public void Toggle()
    {
        _ = _initialized ? true : throw new InvalidOperationException();

        try
        {
            if (!IsActive)
                Apply();
            else
                Revert();
        }
        catch (Win32Exception ex)
        {
            throw new GamePatchException("A Win32 API error ocurred.", ex);
        }

        IsActive = !IsActive;
    }

    protected abstract void Apply();

    protected abstract void Revert();

    public override string ToString()
    {
        return $"{{Name: {GetType().Name}, Process: {Process}, IsActive: {IsActive}}}";
    }
}
