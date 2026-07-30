namespace POPYBot.Plugins;

using System.Collections.Concurrent;

public class PluginHookManager : IPluginHookManager
{
    private readonly ConcurrentDictionary<string, List<Delegate>> _hooks = new();

    public void On(string eventName, Delegate handler)
    {
        _hooks.AddOrUpdate(eventName,
            _ => new List<Delegate> { handler },
            (_, list) => { lock (list) { list.Add(handler); } return list; });
    }

    public void Off(string eventName, Delegate handler)
    {
        if (_hooks.TryGetValue(eventName, out var list))
        {
            lock (list) { list.Remove(handler); }
        }
    }

    public async Task InvokeAsync(string eventName, params object?[] args)
    {
        if (_hooks.TryGetValue(eventName, out var list))
        {
            List<Delegate> handlers;
            lock (list) { handlers = new List<Delegate>(list); }

            foreach (var handler in handlers)
            {
                try
                {
                    var result = handler.DynamicInvoke(args);
                    if (result is Task task)
                        await task;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[botpy] Hook '{eventName}' error: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }
    }
}

public static class HookEvents
{
    public const string Ready = "ready";
    public const string Resumed = "resumed";

    public const string GuildCreate = "guild_create";
    public const string GuildUpdate = "guild_update";
    public const string GuildDelete = "guild_delete";
    public const string ChannelCreate = "channel_create";
    public const string ChannelUpdate = "channel_update";
    public const string ChannelDelete = "channel_delete";

    public const string GuildMemberAdd = "guild_member_add";
    public const string GuildMemberUpdate = "guild_member_update";
    public const string GuildMemberRemove = "guild_member_remove";

    public const string MessageCreate = "message_create";
    public const string MessageDelete = "message_delete";
    public const string AtMessageCreate = "at_message_create";
    public const string PublicMessageDelete = "public_message_delete";

    public const string DirectMessageCreate = "direct_message_create";
    public const string DirectMessageDelete = "direct_message_delete";

    public const string MessageReactionAdd = "message_reaction_add";
    public const string MessageReactionRemove = "message_reaction_remove";

    public const string InteractionCreate = "interaction_create";

    public const string MessageAuditPass = "message_audit_pass";
    public const string MessageAuditReject = "message_audit_reject";

    public const string AudioStart = "audio_start";
    public const string AudioFinish = "audio_finish";
    public const string AudioOnMic = "audio_on_mic";
    public const string AudioOffMic = "audio_off_mic";

    public const string GroupAtMessageCreate = "group_at_message_create";
    public const string GroupMessageCreate = "group_message_create";
    public const string C2CMessageCreate = "c2c_message_create";
    public const string GroupAddRobot = "group_add_robot";
    public const string GroupDelRobot = "group_del_robot";
    public const string GroupMsgReject = "group_msg_reject";
    public const string GroupMsgReceive = "group_msg_receive";
    public const string GroupMemberAdd = "group_member_add";
    public const string GroupMemberRemove = "group_member_remove";
    public const string FriendAdd = "friend_add";
    public const string FriendDel = "friend_del";
    public const string C2CMsgReject = "c2c_msg_reject";
    public const string C2CMsgReceive = "c2c_msg_receive";

    public const string ForumThreadCreate = "forum_thread_create";
    public const string ForumThreadUpdate = "forum_thread_update";
    public const string ForumThreadDelete = "forum_thread_delete";
    public const string ForumPostCreate = "forum_post_create";
    public const string ForumPostDelete = "forum_post_delete";
    public const string ForumReplyCreate = "forum_reply_create";
    public const string ForumReplyDelete = "forum_reply_delete";
    public const string ForumPublishAuditResult = "forum_publish_audit_result";

    public const string AudioOrLiveChannelMemberEnter = "audio_or_live_channel_member_enter";
    public const string AudioOrLiveChannelMemberExit = "audio_or_live_channel_member_exit";

    public const string OpenForumThreadCreate = "open_forum_thread_create";
    public const string OpenForumThreadUpdate = "open_forum_thread_update";
    public const string OpenForumThreadDelete = "open_forum_thread_delete";
    public const string OpenForumPostCreate = "open_forum_post_create";
    public const string OpenForumPostDelete = "open_forum_post_delete";
    public const string OpenForumReplyCreate = "open_forum_reply_create";
    public const string OpenForumReplyDelete = "open_forum_reply_delete";
}
