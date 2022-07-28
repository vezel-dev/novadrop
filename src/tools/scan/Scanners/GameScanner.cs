namespace Vezel.Novadrop.Scanners;

internal abstract class GameScanner
{
    public abstract Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken);
}
