namespace POPYBot.Types;

using System.Text.Json.Serialization;

public class Session
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("last_seq")]
    public long LastSeq { get; set; } = 0;

    [JsonPropertyName("intent")]
    public uint Intent { get; set; }

    [JsonIgnore]
    public Token? Token { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("shards")]
    public ShardConfig Shards { get; set; } = new();
}

public class ShardConfig
{
    [JsonPropertyName("shard_id")]
    public int ShardId { get; set; }

    [JsonPropertyName("shard_count")]
    public int ShardCount { get; set; }
}
