namespace POPYBot.Types;

using System.Text.Json.Serialization;
using System.Text.Json;

public class WsUrlPayload
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";
}

public class WsApInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("shards")]
    public int Shards { get; set; }

    [JsonPropertyName("session_start_limit")]
    public SessionStartLimit SessionStartLimit { get; set; } = new();
}

public class SessionStartLimit
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("remaining")]
    public int Remaining { get; set; }

    [JsonPropertyName("reset_after")]
    public int ResetAfter { get; set; }

    [JsonPropertyName("max_concurrency")]
    public int MaxConcurrency { get; set; }
}

public class ReadyEvent
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = "";

    [JsonPropertyName("user")]
    public GatewayUser User { get; set; } = new();

    [JsonPropertyName("shard")]
    public List<int> Shard { get; set; } = new();
}

public class GatewayUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}

public class MessagePayload
{
    [JsonPropertyName("author")]
    public GatewayUser? Author { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("member")]
    public GatewayMember? Member { get; set; }

    [JsonPropertyName("message_reference")]
    public MessageRef? MessageReference { get; set; }

    [JsonPropertyName("mentions")]
    public List<GatewayUser>? Mentions { get; set; }

    [JsonPropertyName("attachments")]
    public List<MessageAttachment>? Attachments { get; set; }

    // server-inter / callback fields
    [JsonPropertyName("msg_type")]
    public int? MsgType { get; set; }

    [JsonPropertyName("msg_id")]
    public string? MsgId { get; set; }

    [JsonPropertyName("msg_seq")]
    public int? MsgSeq { get; set; }

    [JsonPropertyName("message_type")]
    public int? MessageType { get; set; }

    [JsonPropertyName("msg_elements")]
    public List<MsgElement>? MsgElements { get; set; }

    [JsonPropertyName("ark_data")]
    public JsonElement? ArkData { get; set; }

    [JsonPropertyName("seq")]
    public int? Seq { get; set; }

    [JsonPropertyName("seq_in_channel")]
    public string? SeqInChannel { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}

public class DirectMessagePayload : MessagePayload
{
    [JsonPropertyName("direct_message")]
    public bool DirectMessage { get; set; }

    [JsonPropertyName("src_guild_id")]
    public string? SrcGuildId { get; set; }
}

public class GroupMessagePayload : MessagePayload
{
    [JsonPropertyName("group_openid")]
    public string? GroupOpenid { get; set; }
}

public class GatewayMember
{
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    [JsonPropertyName("joined_at")]
    public string? JoinedAt { get; set; }
}

public class MessageRef
{
    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }
}

public class MessageAttachment
{
    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class MsgElement
{
    [JsonPropertyName("msg_type")]
    public int? MsgType { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("attachments")]
    public List<MessageAttachment>? Attachments { get; set; }

    [JsonPropertyName("message_reference")]
    public MessageRef? MessageReference { get; set; }
}

public class MessageAuditPayload
{
    [JsonPropertyName("audit_id")]
    public string? AuditId { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("audit_time")]
    public string? AuditTime { get; set; }

    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("seq_in_channel")]
    public string? SeqInChannel { get; set; }
}
