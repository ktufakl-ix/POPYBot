using System.Text.Json;
using POPYBot.Types;

namespace POPYBot;

public class BotAPI
{
    private readonly BotHttp _http;

    public BotAPI(BotHttp http)
    {
        _http = http;
    }

    public async Task<WsApInfo?> GetWsUrlAsync() => await _http.GetWsUrlAsync();

    // ===== Guild / 频道 =====
    public async Task<T?> GetGuildAsync<T>(string guildId)
        => await _http.RequestAsync<T>(new Route("GET", "/guilds/{guild_id}", parameters: new() { ["guild_id"] = guildId }));

    // ===== Guild Roles / 频道身份组 =====
    public async Task<T?> GetGuildRolesAsync<T>(string guildId)
        => await _http.RequestAsync<T>(new Route("GET", "/guilds/{guild_id}/roles", parameters: new() { ["guild_id"] = guildId }));

    public async Task<T?> CreateGuildRoleAsync<T>(string guildId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/guilds/{guild_id}/roles", parameters: new() { ["guild_id"] = guildId }), fields);

    public async Task<T?> UpdateGuildRoleAsync<T>(string guildId, string roleId, object fields)
        => await _http.RequestAsync<T>(new Route("PATCH", "/guilds/{guild_id}/roles/{role_id}", parameters: new() { ["guild_id"] = guildId, ["role_id"] = roleId }), fields);

    public async Task<T?> DeleteGuildRoleAsync<T>(string guildId, string roleId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/guilds/{guild_id}/roles/{role_id}", parameters: new() { ["guild_id"] = guildId, ["role_id"] = roleId }));

    public async Task<T?> CreateGuildRoleMemberAsync<T>(string guildId, string roleId, string userId, string? channelId = null)
    {
        var payload = new { channel = new { id = channelId } };
        return await _http.RequestAsync<T>(
            new Route("PUT", "/guilds/{guild_id}/members/{user_id}/roles/{role_id}",
                parameters: new() { ["guild_id"] = guildId, ["user_id"] = userId, ["role_id"] = roleId }), payload);
    }

    public async Task<T?> DeleteGuildRoleMemberAsync<T>(string guildId, string roleId, string userId, string? channelId = null)
    {
        var payload = new { channel = new { id = channelId } };
        return await _http.RequestAsync<T>(
            new Route("DELETE", "/guilds/{guild_id}/members/{user_id}/roles/{role_id}",
                parameters: new() { ["guild_id"] = guildId, ["user_id"] = userId, ["role_id"] = roleId }), payload);
    }

    // ===== Member / 成员 =====
    public async Task<T?> GetGuildMemberAsync<T>(string guildId, string userId)
        => await _http.RequestAsync<T>(new Route("GET", "/guilds/{guild_id}/members/{user_id}", parameters: new() { ["guild_id"] = guildId, ["user_id"] = userId }));

    public async Task<T?> DeleteGuildMemberAsync<T>(string guildId, string userId, bool addBlacklist = false, int deleteHistoryMsgDays = 0)
    {
        var payload = new { add_blacklist = addBlacklist, delete_history_msg_days = deleteHistoryMsgDays };
        return await _http.RequestAsync<T>(
            new Route("DELETE", "/guilds/{guild_id}/members/{user_id}",
                parameters: new() { ["guild_id"] = guildId, ["user_id"] = userId }), payload);
    }

    public async Task<T?> GetGuildMembersAsync<T>(string guildId, string after = "0", int limit = 1)
        => await _http.RequestAsync<T>(new Route("GET", "/guilds/{guild_id}/members?after={after}&limit={limit}",
            parameters: new() { ["guild_id"] = guildId, ["after"] = after, ["limit"] = limit.ToString() }));

    // ===== Channel / 子频道 =====
    public async Task<T?> GetChannelAsync<T>(string channelId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}", parameters: new() { ["channel_id"] = channelId }));

    public async Task<T?> GetChannelsAsync<T>(string guildId)
        => await _http.RequestAsync<T>(new Route("GET", "/guilds/{guild_id}/channels", parameters: new() { ["guild_id"] = guildId }));

    public async Task<T?> CreateChannelAsync<T>(string guildId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/guilds/{guild_id}/channels", parameters: new() { ["guild_id"] = guildId }), fields);

    public async Task<T?> UpdateChannelAsync<T>(string channelId, object fields)
        => await _http.RequestAsync<T>(new Route("PATCH", "/channels/{channel_id}", parameters: new() { ["channel_id"] = channelId }), fields);

    public async Task<T?> DeleteChannelAsync<T>(string channelId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}", parameters: new() { ["channel_id"] = channelId }));

    // ===== Message / 消息 =====
    public async Task<T?> GetMessageAsync<T>(string channelId, string msgId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/messages/{message_id}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId }));

    public async Task<object?> PostMessageAsync(string channelId, string msgId,
        string? content = null, string? image = null, object? embed = null, object? ark = null,
        object? messageReference = null, string? imageUrl = null, object? markdown = null, object? keyboard = null,
        object? fileImage = null, string? eventId = null, int? msgSeq = null, int? msgType = null)
    {
        var payload = new Dictionary<string, object?>();
        if (content != null) payload["content"] = content;
        if (image != null) payload["image"] = image;
        if (embed != null) payload["embed"] = embed;
        if (ark != null) payload["ark"] = ark;
        if (messageReference != null) payload["message_reference"] = messageReference;
        if (imageUrl != null) payload["image"] = imageUrl;
        if (markdown != null) payload["markdown"] = markdown;
        if (keyboard != null) payload["keyboard"] = keyboard;
        if (fileImage != null) payload["file_image"] = fileImage;
        if (eventId != null) payload["event_id"] = eventId;
        if (msgSeq.HasValue) payload["msg_seq"] = msgSeq.Value;
        if (msgType.HasValue) payload["msg_type"] = msgType.Value;
        if (msgId != "0") payload["msg_id"] = msgId;

        return await _http.RequestAsync<object>(new Route("POST", "/channels/{channel_id}/messages",
            parameters: new() { ["channel_id"] = channelId }), payload);
    }

    public async Task<object?> PostDmsAsync(string guildId, string msgId,
        string? content = null, string? image = null, object? embed = null, object? ark = null,
        object? messageReference = null, string? imageUrl = null, object? markdown = null, object? keyboard = null,
        int? msgSeq = null, int? msgType = null)
    {
        var payload = new Dictionary<string, object?>();
        if (content != null) payload["content"] = content;
        if (image != null) payload["image"] = image;
        if (embed != null) payload["embed"] = embed;
        if (ark != null) payload["ark"] = ark;
        if (messageReference != null) payload["message_reference"] = messageReference;
        if (imageUrl != null) payload["image"] = imageUrl;
        if (markdown != null) payload["markdown"] = markdown;
        if (keyboard != null) payload["keyboard"] = keyboard;
        if (msgSeq.HasValue) payload["msg_seq"] = msgSeq.Value;
        if (msgType.HasValue) payload["msg_type"] = msgType.Value;
        if (msgId != "0") payload["msg_id"] = msgId;

        return await _http.RequestAsync<object>(new Route("POST", "/dms/{guild_id}/messages",
            parameters: new() { ["guild_id"] = guildId }), payload);
    }

    public async Task<T?> RetractMessageAsync<T>(string channelId, string msgId, bool hideTip = false)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}/messages/{message_id}?hidetip={hidetip}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId, ["hidetip"] = hideTip.ToString().ToLower() }));

    // ===== Group / 群消息 =====
    public async Task<object?> PostGroupMessageAsync(string groupOpenid, string msgId,
        string? content = null, string? image = null, object? embed = null, object? ark = null,
        object? messageReference = null, string? imageUrl = null, object? markdown = null, object? keyboard = null,
        int? msgSeq = null, int? msgType = null)
    {
        var payload = new Dictionary<string, object?>();
        if (content != null) payload["content"] = content;
        if (image != null) payload["image"] = image;
        if (embed != null) payload["embed"] = embed;
        if (ark != null) payload["ark"] = ark;
        if (messageReference != null) payload["message_reference"] = messageReference;
        if (imageUrl != null) payload["image"] = imageUrl;
        if (markdown != null) payload["markdown"] = markdown;
        if (keyboard != null) payload["keyboard"] = keyboard;
        if (msgSeq.HasValue) payload["msg_seq"] = msgSeq.Value;
        if (msgType.HasValue) payload["msg_type"] = msgType.Value;
        if (msgId != "0") payload["msg_id"] = msgId;

        return await _http.RequestAsync<object>(new Route("POST", "/v2/groups/{group_openid}/messages",
            parameters: new() { ["group_openid"] = groupOpenid }), payload);
    }

    // ===== C2C / 单聊消息 =====
    public async Task<object?> PostC2CMessageAsync(string openid, string msgId,
        string? content = null, string? image = null, object? embed = null, object? ark = null,
        object? messageReference = null, string? imageUrl = null, object? markdown = null, object? keyboard = null,
        int? msgSeq = null, int? msgType = null)
    {
        var payload = new Dictionary<string, object?>();
        if (content != null) payload["content"] = content;
        if (image != null) payload["image"] = image;
        if (embed != null) payload["embed"] = embed;
        if (ark != null) payload["ark"] = ark;
        if (messageReference != null) payload["message_reference"] = messageReference;
        if (imageUrl != null) payload["image"] = imageUrl;
        if (markdown != null) payload["markdown"] = markdown;
        if (keyboard != null) payload["keyboard"] = keyboard;
        if (msgSeq.HasValue) payload["msg_seq"] = msgSeq.Value;
        if (msgType.HasValue) payload["msg_type"] = msgType.Value;
        if (msgId != "0") payload["msg_id"] = msgId;

        return await _http.RequestAsync<object>(new Route("POST", "/v2/users/{openid}/messages",
            parameters: new() { ["openid"] = openid }), payload);
    }

    // ===== Private Message / 私信 =====
    public async Task<T?> CreateDmsAsync<T>(string guildId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/users/@me/dms", parameters: new() { ["guild_id"] = guildId }), fields);

    // ===== Pins Message / 精华消息 =====
    public async Task<T?> AddPinsAsync<T>(string channelId, string msgId)
        => await _http.RequestAsync<T>(new Route("PUT", "/channels/{channel_id}/pins/{message_id}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId }));

    public async Task<T?> DeletePinsAsync<T>(string channelId, string msgId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}/pins/{message_id}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId }));

    public async Task<T?> GetPinsAsync<T>(string channelId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/pins",
            parameters: new() { ["channel_id"] = channelId }));

    // ===== Announce / 公告 =====
    public async Task<T?> CreateAnnounceAsync<T>(string guildId, string channelId, string msgId)
    {
        var payload = new { message_id = msgId, channel_id = channelId };
        return await _http.RequestAsync<T>(new Route("POST", "/guilds/{guild_id}/announces",
            parameters: new() { ["guild_id"] = guildId }), payload);
    }

    public async Task<T?> DeleteAnnounceAsync<T>(string guildId, string msgId = "all")
        => await _http.RequestAsync<T>(new Route("DELETE", "/guilds/{guild_id}/announces/{message_id}",
            parameters: new() { ["guild_id"] = guildId, ["message_id"] = msgId }));

    // ===== Schedule / 日程 =====
    public async Task<T?> GetSchedulesAsync<T>(string channelId, string since = "")
    {
        var route = new Route("GET", "/channels/{channel_id}/schedules?since={since}",
            parameters: new() { ["channel_id"] = channelId, ["since"] = since });
        return await _http.RequestAsync<T>(route);
    }

    public async Task<T?> GetScheduleAsync<T>(string channelId, string scheduleId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/schedules/{schedule_id}",
            parameters: new() { ["channel_id"] = channelId, ["schedule_id"] = scheduleId }));

    public async Task<T?> CreateScheduleAsync<T>(string channelId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/channels/{channel_id}/schedules",
            parameters: new() { ["channel_id"] = channelId }), fields);

    public async Task<T?> UpdateScheduleAsync<T>(string channelId, string scheduleId, object fields)
        => await _http.RequestAsync<T>(new Route("PATCH", "/channels/{channel_id}/schedules/{schedule_id}",
            parameters: new() { ["channel_id"] = channelId, ["schedule_id"] = scheduleId }), fields);

    public async Task<T?> DeleteScheduleAsync<T>(string channelId, string scheduleId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}/schedules/{schedule_id}",
            parameters: new() { ["channel_id"] = channelId, ["schedule_id"] = scheduleId }));

    // ===== Reaction / 表情表态 =====
    public async Task<T?> PutReactionAsync<T>(string channelId, string msgId, int emojiType, string emojiId)
    {
        var payload = new { emoji_type = emojiType, emoji_id = emojiId };
        return await _http.RequestAsync<T>(new Route("PUT", "/channels/{channel_id}/messages/{message_id}/reactions/{type}/{id}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId, ["type"] = emojiType.ToString(), ["id"] = emojiId }), payload);
    }

    public async Task<T?> DeleteReactionAsync<T>(string channelId, string msgId, int emojiType, string emojiId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}/messages/{message_id}/reactions/{type}/{id}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId, ["type"] = emojiType.ToString(), ["id"] = emojiId }));

    public async Task<T?> GetReactionUsersAsync<T>(string channelId, string msgId, int emojiType, string emojiId,
        string cookie = "", int limit = 20)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/messages/{message_id}/reactions/{type}/{id}?cookie={cookie}&limit={limit}",
            parameters: new() { ["channel_id"] = channelId, ["message_id"] = msgId, ["type"] = emojiType.ToString(),
                ["id"] = emojiId, ["cookie"] = cookie, ["limit"] = limit.ToString() }));

    // ===== Audio / 音频 =====
    public async Task<T?> ControlAudioAsync<T>(string channelId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/channels/{channel_id}/audio",
            parameters: new() { ["channel_id"] = channelId }), fields);

    public async Task<T?> PushMicAsync<T>(string channelId, object fields)
        => await _http.RequestAsync<T>(new Route("POST", "/channels/{channel_id}/mic",
            parameters: new() { ["channel_id"] = channelId }), fields);

    // ===== Permission / 权限 =====
    public async Task<T?> GetChannelPermissionsAsync<T>(string channelId, string userId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/members/{user_id}/permissions",
            parameters: new() { ["channel_id"] = channelId, ["user_id"] = userId }));

    public async Task<T?> UpdateChannelPermissionsAsync<T>(string channelId, string userId, object fields)
        => await _http.RequestAsync<T>(new Route("PUT", "/channels/{channel_id}/members/{user_id}/permissions",
            parameters: new() { ["channel_id"] = channelId, ["user_id"] = userId }), fields);

    public async Task<T?> GetRoleChannelPermissionsAsync<T>(string channelId, string roleId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/roles/{role_id}/permissions",
            parameters: new() { ["channel_id"] = channelId, ["role_id"] = roleId }));

    public async Task<T?> UpdateRoleChannelPermissionsAsync<T>(string channelId, string roleId, object fields)
        => await _http.RequestAsync<T>(new Route("PUT", "/channels/{channel_id}/roles/{role_id}/permissions",
            parameters: new() { ["channel_id"] = channelId, ["role_id"] = roleId }), fields);

    // ===== Forum / 论坛 =====
    public async Task<T?> GetThreadsAsync<T>(string channelId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/threads",
            parameters: new() { ["channel_id"] = channelId }));

    public async Task<T?> GetThreadAsync<T>(string channelId, string threadId)
        => await _http.RequestAsync<T>(new Route("GET", "/channels/{channel_id}/threads/{thread_id}",
            parameters: new() { ["channel_id"] = channelId, ["thread_id"] = threadId }));

    public async Task<T?> PostThreadAsync<T>(string channelId, object fields)
        => await _http.RequestAsync<T>(new Route("PUT", "/channels/{channel_id}/threads",
            parameters: new() { ["channel_id"] = channelId }), fields);

    public async Task<T?> DeleteThreadAsync<T>(string channelId, string threadId)
        => await _http.RequestAsync<T>(new Route("DELETE", "/channels/{channel_id}/threads/{thread_id}",
            parameters: new() { ["channel_id"] = channelId, ["thread_id"] = threadId }));
}
