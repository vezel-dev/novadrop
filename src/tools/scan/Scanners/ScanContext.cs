namespace Vezel.Novadrop.Scanners;

sealed class ScanContext
{
    public MemoryWindow Window { get; }

    public DirectoryInfo Output { get; }

    public ScanContext(MemoryWindow window, DirectoryInfo output)
    {
        Window = window;
        Output = output;
    }
}
