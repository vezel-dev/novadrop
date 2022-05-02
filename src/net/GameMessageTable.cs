namespace Vezel.Novadrop.Net;

public static class GameMessageTable
{
    public static ImmutableDictionary<ushort, string> CodeToName { get; }

    public static ImmutableDictionary<string, ushort> NameToCode { get; }

    static GameMessageTable()
    {
        NameToCode = new Dictionary<string, ushort>
        {
            // TODO
        }.ToImmutableDictionary();

        CodeToName = NameToCode.ToImmutableDictionary(x => x.Value, x => x.Key);
    }
}
