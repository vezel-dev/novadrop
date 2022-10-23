namespace Vezel.Novadrop.Resources;

public sealed class ResourceContainerLoadOptions
{
    public ReadOnlyMemory<byte> Key { get; private set; } = ResourceContainer.LatestKey;

    public bool Strict { get; private set; }

    private ResourceContainerLoadOptions Clone()
    {
        return new()
        {
            Key = Key,
            Strict = Strict,
        };
    }

    public ResourceContainerLoadOptions WithKey(scoped ReadOnlySpan<byte> key)
    {
        Check.Argument(key.Length == ResourceContainer.LatestKey.Length, nameof(key));

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
