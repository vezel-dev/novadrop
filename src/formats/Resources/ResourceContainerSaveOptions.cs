namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainerSaveOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = ResourceContainer.LatestKey;

    ResourceContainerSaveOptions Clone()
    {
        return new()
        {
            Key = Key,
        };
    }

    public ResourceContainerSaveOptions WithKey(ReadOnlySpan<byte> key)
    {
        _ = key.Length == ResourceContainer.LatestKey.Length ? true : throw new ArgumentException(null, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }
}
