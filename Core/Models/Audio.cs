namespace POPYBot.Models;

public class Audio
{
    public BotAPI Api { get; }
    public string? ChannelId { get; }
    public string? GuildId { get; }
    public string? AudioUrl { get; }
    public string? Text { get; }
    public string? EventId { get; }

    public Audio(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();
        AudioUrl = data.GetValueOrDefault("audio_url")?.ToString();
        Text = data.GetValueOrDefault("text")?.ToString();
    }
}

public class PublicAudio
{
    public BotAPI Api { get; }
    public string? GuildId { get; }
    public string? ChannelId { get; }
    public int? ChannelType { get; }
    public string? UserId { get; }

    public PublicAudio(BotAPI api, Dictionary<string, object?> data)
    {
        Api = api;
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        ChannelType = Convert.ToInt32(data.GetValueOrDefault("channel_type"));
        UserId = data.GetValueOrDefault("user_id")?.ToString();
    }
}
