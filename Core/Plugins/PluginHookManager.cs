namespace POPYBot.Plugins;

using System.Collections.Concurrent;
using POPYBot.Models;
using ThreadModel = POPYBot.Models.Thread;   // resolves ambiguity with System.Threading.Thread

public class PluginHookManager : IPluginHookManager
{
    private readonly ConcurrentDictionary<string, List<Delegate>> _hooks = new();

    // ── Typed events ──

    public event Func<Task> OnReady           { add => On("ready",               value); remove => Off("ready",               value); }
    public event Func<Task> OnResumed          { add => On("resumed",             value); remove => Off("resumed",             value); }

    public event Func<Guild, Task>   OnGuildCreate    { add => On("guild_create",    value); remove => Off("guild_create",    value); }
    public event Func<Guild, Task>   OnGuildUpdate    { add => On("guild_update",    value); remove => Off("guild_update",    value); }
    public event Func<Guild, Task>   OnGuildDelete    { add => On("guild_delete",    value); remove => Off("guild_delete",    value); }
    public event Func<Channel, Task> OnChannelCreate  { add => On("channel_create",  value); remove => Off("channel_create",  value); }
    public event Func<Channel, Task> OnChannelUpdate  { add => On("channel_update",  value); remove => Off("channel_update",  value); }
    public event Func<Channel, Task> OnChannelDelete  { add => On("channel_delete",  value); remove => Off("channel_delete",  value); }

    public event Func<Member, Task> OnGuildMemberAdd    { add => On("guild_member_add",    value); remove => Off("guild_member_add",    value); }
    public event Func<Member, Task> OnGuildMemberUpdate { add => On("guild_member_update", value); remove => Off("guild_member_update", value); }
    public event Func<Member, Task> OnGuildMemberRemove { add => On("guild_member_remove", value); remove => Off("guild_member_remove", value); }

    public event Func<Message, Task> OnMessageCreate       { add => On("message_create",       value); remove => Off("message_create",       value); }
    public event Func<Message, Task> OnMessageDelete       { add => On("message_delete",       value); remove => Off("message_delete",       value); }
    public event Func<Message, Task> OnAtMessageCreate     { add => On("at_message_create",    value); remove => Off("at_message_create",    value); }
    public event Func<Message, Task> OnPublicMessageDelete { add => On("public_message_delete", value); remove => Off("public_message_delete", value); }

    public event Func<DirectMessage, Task> OnDirectMessageCreate { add => On("direct_message_create", value); remove => Off("direct_message_create", value); }
    public event Func<DirectMessage, Task> OnDirectMessageDelete { add => On("direct_message_delete", value); remove => Off("direct_message_delete", value); }

    public event Func<Reaction, Task> OnMessageReactionAdd    { add => On("message_reaction_add",    value); remove => Off("message_reaction_add",    value); }
    public event Func<Reaction, Task> OnMessageReactionRemove { add => On("message_reaction_remove", value); remove => Off("message_reaction_remove", value); }

    public event Func<Interaction, Task> OnInteractionCreate { add => On("interaction_create", value); remove => Off("interaction_create", value); }

    public event Func<MessageAudit, Task> OnMessageAuditPass   { add => On("message_audit_pass",   value); remove => Off("message_audit_pass",   value); }
    public event Func<MessageAudit, Task> OnMessageAuditReject { add => On("message_audit_reject", value); remove => Off("message_audit_reject", value); }

    public event Func<Audio, Task> OnAudioStart  { add => On("audio_start",  value); remove => Off("audio_start",  value); }
    public event Func<Audio, Task> OnAudioFinish { add => On("audio_finish", value); remove => Off("audio_finish", value); }
    public event Func<Audio, Task> OnAudioOnMic  { add => On("audio_on_mic", value); remove => Off("audio_on_mic", value); }
    public event Func<Audio, Task> OnAudioOffMic { add => On("audio_off_mic", value); remove => Off("audio_off_mic", value); }

    public event Func<GroupMessage, Task> OnGroupAtMessageCreate { add => On("group_at_message_create", value); remove => Off("group_at_message_create", value); }
    public event Func<GroupMessage, Task> OnGroupMessageCreate   { add => On("group_message_create",   value); remove => Off("group_message_create",   value); }
    public event Func<C2CMessage, Task>   OnC2CMessageCreate     { add => On("c2c_message_create",     value); remove => Off("c2c_message_create",     value); }

    public event Func<GroupManageEvent, Task> OnGroupAddRobot      { add => On("group_add_robot",      value); remove => Off("group_add_robot",      value); }
    public event Func<GroupManageEvent, Task> OnGroupDelRobot      { add => On("group_del_robot",      value); remove => Off("group_del_robot",      value); }
    public event Func<GroupManageEvent, Task> OnGroupMsgReject     { add => On("group_msg_reject",     value); remove => Off("group_msg_reject",     value); }
    public event Func<GroupManageEvent, Task> OnGroupMsgReceive    { add => On("group_msg_receive",    value); remove => Off("group_msg_receive",    value); }
    public event Func<GroupManageEvent, Task> OnGroupMemberAdd     { add => On("group_member_add",     value); remove => Off("group_member_add",     value); }
    public event Func<GroupManageEvent, Task> OnGroupMemberRemove  { add => On("group_member_remove",  value); remove => Off("group_member_remove",  value); }

    public event Func<C2CManageEvent, Task> OnFriendAdd     { add => On("friend_add",     value); remove => Off("friend_add",     value); }
    public event Func<C2CManageEvent, Task> OnFriendDel     { add => On("friend_del",     value); remove => Off("friend_del",     value); }
    public event Func<C2CManageEvent, Task> OnC2CMsgReject  { add => On("c2c_msg_reject", value); remove => Off("c2c_msg_reject", value); }
    public event Func<C2CManageEvent, Task> OnC2CMsgReceive { add => On("c2c_msg_receive", value); remove => Off("c2c_msg_receive", value); }

    public event Func<ThreadModel, Task> OnForumThreadCreate { add => On("forum_thread_create", value); remove => Off("forum_thread_create", value); }
    public event Func<ThreadModel, Task> OnForumThreadUpdate { add => On("forum_thread_update", value); remove => Off("forum_thread_update", value); }
    public event Func<ThreadModel, Task> OnForumThreadDelete { add => On("forum_thread_delete", value); remove => Off("forum_thread_delete", value); }

    public event Func<Dictionary<string, object?>, Task> OnForumPostCreate         { add => On("forum_post_create",         value); remove => Off("forum_post_create",         value); }
    public event Func<Dictionary<string, object?>, Task> OnForumPostDelete         { add => On("forum_post_delete",         value); remove => Off("forum_post_delete",         value); }
    public event Func<Dictionary<string, object?>, Task> OnForumReplyCreate        { add => On("forum_reply_create",        value); remove => Off("forum_reply_create",        value); }
    public event Func<Dictionary<string, object?>, Task> OnForumReplyDelete        { add => On("forum_reply_delete",        value); remove => Off("forum_reply_delete",        value); }
    public event Func<Dictionary<string, object?>, Task> OnForumPublishAuditResult { add => On("forum_publish_audit_result", value); remove => Off("forum_publish_audit_result", value); }

    public event Func<PublicAudio, Task> OnAudioOrLiveChannelMemberEnter { add => On("audio_or_live_channel_member_enter", value); remove => Off("audio_or_live_channel_member_enter", value); }
    public event Func<PublicAudio, Task> OnAudioOrLiveChannelMemberExit  { add => On("audio_or_live_channel_member_exit",  value); remove => Off("audio_or_live_channel_member_exit",  value); }

    public event Func<OpenThread, Task> OnOpenForumThreadCreate { add => On("open_forum_thread_create", value); remove => Off("open_forum_thread_create", value); }
    public event Func<OpenThread, Task> OnOpenForumThreadUpdate { add => On("open_forum_thread_update", value); remove => Off("open_forum_thread_update", value); }
    public event Func<OpenThread, Task> OnOpenForumThreadDelete { add => On("open_forum_thread_delete", value); remove => Off("open_forum_thread_delete", value); }

    public event Func<Dictionary<string, object?>, Task> OnOpenForumPostCreate  { add => On("open_forum_post_create",  value); remove => Off("open_forum_post_create",  value); }
    public event Func<Dictionary<string, object?>, Task> OnOpenForumPostDelete  { add => On("open_forum_post_delete",  value); remove => Off("open_forum_post_delete",  value); }
    public event Func<Dictionary<string, object?>, Task> OnOpenForumReplyCreate { add => On("open_forum_reply_create", value); remove => Off("open_forum_reply_create", value); }
    public event Func<Dictionary<string, object?>, Task> OnOpenForumReplyDelete { add => On("open_forum_reply_delete", value); remove => Off("open_forum_reply_delete", value); }

    // ── Core registration ──

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
