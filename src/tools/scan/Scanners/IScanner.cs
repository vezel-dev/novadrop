namespace Vezel.Novadrop.Scanners;

interface IScanner
{
    Task RunAsync(ScanContext context);
}
