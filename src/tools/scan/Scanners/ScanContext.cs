namespace Vezel.Novadrop.Scanners;

sealed class ScanContext
{
    public NativeProcess Process { get; }

    public DirectoryInfo Output { get; }

    public ScanContext(NativeProcess process, DirectoryInfo output)
    {
        Process = process;
        Output = output;
    }
}
