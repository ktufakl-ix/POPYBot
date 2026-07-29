namespace POPYBot.Models;

public class Reaction
{
    public BotAPI Api { get; }
    public string? UserId { get; }
    public string? ChannelId { get; }
    public string? GuildId { get; }
    public ReactionEmoji? Emoji { get; }
    public ReactionTarget? Target { get; }
    public string? EventId { get; }

    public Reaction(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        UserId = data.GetValueOrDefault("user_id")?.ToString();
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();

        if (data.TryGetValue("emoji", out var e) && e is Dictionary<string, object?> emojiData)
            Emoji = new ReactionEmoji(emojiData);
        if (data.TryGetValue("target", out var t) && t is Dictionary<string, object?> targetData)
            Target = new ReactionTarget(targetData);
    }
}

public class ReactionEmoji
{
    public string? Id { get; }
    public int? Type { get; }

    public ReactionEmoji(Dictionary<string, object?> data)
    {
        Id = data.GetValueOrDefault("id")?.ToString();
        Type = Convert.ToInt32(data.GetValueOrDefault("type"));
    }
}

public class ReactionTarget
{
    public string? Id { get; }
    public int? Type { get; }  // 0: 消息 1: 帖子 2: 评论 3: 回复

    public ReactionTarget(Dictionary<string, object?> data)
    {
        Id = data.GetValueOrDefault("id")?.ToString();
        Type = Convert.ToInt32(data.GetValueOrDefault("type"));
    }
}
