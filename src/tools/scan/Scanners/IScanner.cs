namespace Vezel.Novadrop.Scanners;

interface IScanner
{
    Task<bool> RunAsync(ScanContext context, CancellationToken cancellationToken);
}
