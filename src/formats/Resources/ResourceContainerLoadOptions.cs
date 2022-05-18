namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainerLoadOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = ResourceContainer.LatestKey;

    public bool Strict { get; private set; }

    ResourceContainerLoadOptions Clone()
    {
        return new()
        {
            Key = Key,
            Strict = Strict,
        };
    }

    public ResourceContainerLoadOptions WithKey(ReadOnlySpan<byte> key)
    {
        _ = key.Length == ResourceContainer.LatestKey.Length ? true : throw new ArgumentException(null, nameof(key));

        var options = Clone();

        options.Key = key.ToArray();

        return options;
    }

    public ResourceContainerLoadOptions WithStrict(bool strict)
    {
        var options = Clone();

        options.Strict = strict;

        return options;
    }
}
