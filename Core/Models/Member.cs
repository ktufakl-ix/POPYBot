using System.Text.Json;
using POPYBot.Types;

namespace POPYBot.Models;

public class Member
{
    public BotAPI Api { get; }
    public MemberUser User { get; }
    public string? Nick { get; }
    public List<string>? Roles { get; }
    public string? JoinedAt { get; }
    public string? EventId { get; }
    public string? GuildId { get; }

    public Member(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;

        var userData = TryGetDict(data, "user");
        User = new MemberUser(userData);
        Nick = data.GetValueOrDefault("nick")?.ToString();
        Roles = (data.GetValueOrDefault("roles") as System.Text.Json.JsonElement?)?.Deserialize<List<string>>();
        JoinedAt = data.GetValueOrDefault("joined_at")?.ToString();
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();
    }

    private static Dictionary<string, object?> TryGetDict(Dictionary<string, object?> data, string key)
    {
        if (data.TryGetValue(key, out var val) && val is Dictionary<string, object?> dict)
            return dict;
        return new();
    }
}

public class MemberUser
{
    public string? Id { get; }
    public string? Username { get; }
    public string? Avatar { get; }
    public bool? Bot { get; }
    public string? UnionOpenid { get; }
    public string? UnionUserAccount { get; }

    public MemberUser(Dictionary<string, object?> data)
    {
        Id = data.GetValueOrDefault("id")?.ToString();
        Username = data.GetValueOrDefault("username")?.ToString();
        Avatar = data.GetValueOrDefault("avatar")?.ToString();
        Bot = data.GetValueOrDefault("bot") as bool?;
        UnionOpenid = data.GetValueOrDefault("union_openid")?.ToString();
        UnionUserAccount = data.GetValueOrDefault("union_user_account")?.ToString();
    }
}
