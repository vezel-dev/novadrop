namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainerEntry
{
    public ResourceContainer Container { get; }

    public string Name { get; }

    public Memory<byte> Data { get; set; }

    internal ResourceContainerEntry(ResourceContainer container, string name)
    {
        Container = container;
        Name = name;
    }

    public override string ToString()
    {
        return $"{{Name: {Name}, Data: [{Data.Length}]}}";
    }
}
