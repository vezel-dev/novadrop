namespace Vezel.Novadrop.Net;

public sealed class NetworkPacketTable
{
    public static NetworkPacketTable LatestTable { get; } =
        new(387463, new Dictionary<string, ushort>
        {
            // TODO: Fill this in.
        });

    public int Revision { get; }

    public IReadOnlyDictionary<string, ushort> NameToCode { get; }

    public IReadOnlyDictionary<ushort, string> CodeToName { get; }

    public NetworkPacketTable(int revision, IReadOnlyDictionary<string, ushort> mapping)
    {
        _ = revision >= 0 ? true : throw new ArgumentOutOfRangeException(nameof(revision));
        ArgumentNullException.ThrowIfNull(mapping);
        _ = mapping.Keys.All(k => k != null) ? true : throw new ArgumentException(null, nameof(mapping));

        Revision = revision;
        NameToCode = mapping.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        CodeToName = mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }
}
