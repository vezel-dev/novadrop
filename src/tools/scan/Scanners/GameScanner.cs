namespace Vezel.Novadrop.Scanners;

abstract class GameScanner
{
    public abstract Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken);
}
