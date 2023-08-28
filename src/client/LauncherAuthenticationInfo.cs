namespace Vezel.Novadrop.Client;

internal sealed class LauncherAuthenticationInfo
{
    // TODO: access_level, account_bits, user_permission

    public sealed class ServerCharacters
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        [JsonPropertyName("char_count")]
        public int Characters { get; }

        public ServerCharacters(int id, int characters)
        {
            Id = id;
            Characters = characters;
        }
    }

    [JsonPropertyName("result-code")]
    public int ResultCode { get; } = 200;

    [JsonPropertyName("game_account_name")]
    public string GameAccountName { get; } = "TERA";

    [JsonPropertyName("master_account_name")]
    public string MasterAccountName { get; }

    [JsonPropertyName("ticket")]
    public string SessionTicket { get; }

    [JsonPropertyName("chars_per_server")]
    public IReadOnlyCollection<ServerCharacters> CharactersPerServer { get; }

    [JsonPropertyName("last_connected_server_id")]
    public int LastServerId { get; }

    public LauncherAuthenticationInfo(
        string accountName, string sessionTicket, IEnumerable<ServerCharacters> charactersPerServer, int lastServerId)
    {
        MasterAccountName = accountName;
        SessionTicket = sessionTicket;
        CharactersPerServer = charactersPerServer.ToArray();
        LastServerId = lastServerId;
    }
}
