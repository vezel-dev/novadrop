namespace Vezel.Novadrop.Memory;

public sealed class NativeModule
{
    public string Name { get; }

    public MemoryWindow Window { get; }

    internal NativeModule(string name, MemoryWindow window)
    {
        Name = name;
        Window = window;
    }

    public override string ToString()
    {
        return $"{{Name: {Name}, Window: {Window}}}";
    }
}
