namespace Vezel.Novadrop.Client;

sealed class LauncherGameInfo
{
    // TODO: account_bits, access_level, chars_per_server, user_permission

    [JsonPropertyName("result-code")]
    public int ResultCode { get; } = 200;

    [JsonPropertyName("game_account_name")]
    public string GameAccountName { get; } = "TERA";

    [JsonPropertyName("master_account_name")]
    public string MasterAccountName { get; }

    [JsonPropertyName("ticket")]
    public string Ticket { get; }

    [JsonPropertyName("last_connected_server_id")]
    public int LastServerId { get; }

    public LauncherGameInfo(string accountName, string ticket, int lastServerId)
    {
        MasterAccountName = accountName;
        Ticket = ticket;
        LastServerId = lastServerId;
    }
}
