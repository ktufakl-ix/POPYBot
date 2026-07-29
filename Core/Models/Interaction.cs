namespace POPYBot.Models;

public class Interaction
{
    public BotAPI Api { get; }
    public string? Id { get; }
    public string? ApplicationId { get; }
    public int? Type { get; }
    public string? Scene { get; }
    public int? ChatType { get; }
    public string? EventId { get; }
    public InteractionData? Data { get; }
    public string? GuildId { get; }
    public string? ChannelId { get; }
    public string? UserOpenid { get; }
    public string? GroupOpenid { get; }
    public string? GroupMemberOpenid { get; }
    public string? Timestamp { get; }
    public int? Version { get; }

    public Interaction(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        Id = data.GetValueOrDefault("id")?.ToString();
        ApplicationId = data.GetValueOrDefault("application_id")?.ToString();
        Type = Convert.ToInt32(data.GetValueOrDefault("type"));
        Scene = data.GetValueOrDefault("scene")?.ToString();
        ChatType = Convert.ToInt32(data.GetValueOrDefault("chat_type"));
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        UserOpenid = data.GetValueOrDefault("user_openid")?.ToString();
        GroupOpenid = data.GetValueOrDefault("group_openid")?.ToString();
        GroupMemberOpenid = data.GetValueOrDefault("group_member_openid")?.ToString();
        Timestamp = data.GetValueOrDefault("timestamp")?.ToString();
        Version = Convert.ToInt32(data.GetValueOrDefault("version"));

        if (data.TryGetValue("data", out var d) && d is Dictionary<string, object?> interactionData)
        {
            Data = new InteractionData(interactionData);
        }
    }
}

public class InteractionData
{
    public int? Type { get; }
    public InteractionResolved? Resolved { get; }

    public InteractionData(Dictionary<string, object?> data)
    {
        Type = Convert.ToInt32(data.GetValueOrDefault("type"));
        if (data.TryGetValue("resolved", out var r) && r is Dictionary<string, object?> resolved)
        {
            Resolved = new InteractionResolved(resolved);
        }
    }
}

public class InteractionResolved
{
    public string? ButtonId { get; }
    public string? ButtonData { get; }
    public string? MessageId { get; }
    public string? UserId { get; }
    public string? FeatureId { get; }

    public InteractionResolved(Dictionary<string, object?> data)
    {
        ButtonId = data.GetValueOrDefault("button_id")?.ToString();
        ButtonData = data.GetValueOrDefault("button_data")?.ToString();
        MessageId = data.GetValueOrDefault("message_id")?.ToString();
        UserId = data.GetValueOrDefault("user_id")?.ToString();
        FeatureId = data.GetValueOrDefault("feature_id")?.ToString();
    }
}
