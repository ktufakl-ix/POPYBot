namespace POPYBot.Models;

public class GroupManageEvent
{
    public BotAPI Api { get; }
    public string? EventId { get; }
    public string? Timestamp { get; }
    public string? GroupOpenid { get; }
    public string? OpMemberOpenid { get; }

    public GroupManageEvent(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        Timestamp = data.GetValueOrDefault("timestamp")?.ToString();
        GroupOpenid = data.GetValueOrDefault("group_openid")?.ToString();
        OpMemberOpenid = data.GetValueOrDefault("op_member_openid")?.ToString();
    }
}

public class C2CManageEvent
{
    public BotAPI Api { get; }
    public string? EventId { get; }
    public string? Timestamp { get; }
    public string? Openid { get; }

    public C2CManageEvent(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        Timestamp = data.GetValueOrDefault("timestamp")?.ToString();
        Openid = data.GetValueOrDefault("openid")?.ToString();
    }
}
