namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainerSaveOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = ResourceContainer.LatestKey;

    private ResourceContainerSaveOptions Clone()
    {
        return new()
        {
            Key = Key,
        };
    }

    public ResourceContainerSaveOptions WithKey(scoped ReadOnlySpan<byte> key)
    {
        Check.Argument(key.Length == ResourceContainer.LatestKey.Length, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }
}
