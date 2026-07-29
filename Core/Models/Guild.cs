using System.Text.Json;
using POPYBot.Types;

namespace POPYBot.Models;

public class Guild
{
    public BotAPI Api { get; }
    public string? Id { get; }
    public string? Name { get; }
    public string? Icon { get; }
    public string? OwnerId { get; }
    public bool? IsOwner { get; }
    public int? MemberCount { get; }
    public int? MaxMembers { get; }
    public string? Description { get; }
    public string? JoinedAt { get; }
    public string? EventId { get; }

    public Guild(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        Id = data.GetValueOrDefault("id")?.ToString();
        Name = data.GetValueOrDefault("name")?.ToString();
        Icon = data.GetValueOrDefault("icon")?.ToString();
        OwnerId = data.GetValueOrDefault("owner_id")?.ToString();
        IsOwner = data.GetValueOrDefault("owner") as bool?;
        MemberCount = Convert.ToInt32(data.GetValueOrDefault("member_count"));
        MaxMembers = Convert.ToInt32(data.GetValueOrDefault("max_members"));
        Description = data.GetValueOrDefault("description")?.ToString();
        JoinedAt = data.GetValueOrDefault("joined_at")?.ToString();
    }
}

public class Channel
{
    public BotAPI Api { get; }
    public string? Id { get; }
    public string? Name { get; }
    public int? Type { get; }
    public int? SubType { get; }
    public int? Position { get; }
    public string? OwnerId { get; }
    public int? PrivateType { get; }
    public int? SpeakPermission { get; }
    public string? ApplicationId { get; }
    public string? Permissions { get; }
    public string? EventId { get; }

    public Channel(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        Id = data.GetValueOrDefault("id")?.ToString();
        Name = data.GetValueOrDefault("name")?.ToString();
        Type = Convert.ToInt32(data.GetValueOrDefault("type"));
        SubType = Convert.ToInt32(data.GetValueOrDefault("sub_type"));
        Position = Convert.ToInt32(data.GetValueOrDefault("position"));
        OwnerId = data.GetValueOrDefault("owner_id")?.ToString();
        PrivateType = Convert.ToInt32(data.GetValueOrDefault("private_type"));
        SpeakPermission = Convert.ToInt32(data.GetValueOrDefault("speak_permission"));
        ApplicationId = data.GetValueOrDefault("application_id")?.ToString();
        Permissions = data.GetValueOrDefault("permissions")?.ToString();
    }
}
