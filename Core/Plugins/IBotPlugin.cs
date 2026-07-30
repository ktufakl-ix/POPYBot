using POPYBot.Models;
using ThreadModel = POPYBot.Models.Thread;   // resolves ambiguity with System.Threading.Thread

namespace POPYBot.Plugins;

public interface IBotPlugin
{
    string Name { get; }
    string Version { get; }
    Task InitializeAsync(IPluginContext context);
    Task ShutdownAsync();
}

public interface IPluginContext
{
    BotAPI Api { get; }
    IPluginHookManager Hooks { get; }
}

public interface IPluginHookManager
{
    // ── Typed events (subscription: ctx.Hooks.OnReady += () => { ... }) ──

    // Zero-arg lifecycle
    event Func<Task> OnReady;
    event Func<Task> OnResumed;

    // Guild
    event Func<Guild, Task> OnGuildCreate;
    event Func<Guild, Task> OnGuildUpdate;
    event Func<Guild, Task> OnGuildDelete;
    event Func<Channel, Task> OnChannelCreate;
    event Func<Channel, Task> OnChannelUpdate;
    event Func<Channel, Task> OnChannelDelete;

    // Guild member
    event Func<Member, Task> OnGuildMemberAdd;
    event Func<Member, Task> OnGuildMemberUpdate;
    event Func<Member, Task> OnGuildMemberRemove;

    // Message
    event Func<Message, Task> OnMessageCreate;
    event Func<Message, Task> OnMessageDelete;
    event Func<Message, Task> OnAtMessageCreate;
    event Func<Message, Task> OnPublicMessageDelete;

    // Direct message
    event Func<DirectMessage, Task> OnDirectMessageCreate;
    event Func<DirectMessage, Task> OnDirectMessageDelete;

    // Reaction
    event Func<Reaction, Task> OnMessageReactionAdd;
    event Func<Reaction, Task> OnMessageReactionRemove;

    // Interaction
    event Func<Interaction, Task> OnInteractionCreate;

    // Audit
    event Func<MessageAudit, Task> OnMessageAuditPass;
    event Func<MessageAudit, Task> OnMessageAuditReject;

    // Audio
    event Func<Audio, Task> OnAudioStart;
    event Func<Audio, Task> OnAudioFinish;
    event Func<Audio, Task> OnAudioOnMic;
    event Func<Audio, Task> OnAudioOffMic;

    // Group message
    event Func<GroupMessage, Task> OnGroupAtMessageCreate;
    event Func<GroupMessage, Task> OnGroupMessageCreate;
    event Func<C2CMessage, Task> OnC2CMessageCreate;

    // Group manage
    event Func<GroupManageEvent, Task> OnGroupAddRobot;
    event Func<GroupManageEvent, Task> OnGroupDelRobot;
    event Func<GroupManageEvent, Task> OnGroupMsgReject;
    event Func<GroupManageEvent, Task> OnGroupMsgReceive;
    event Func<GroupManageEvent, Task> OnGroupMemberAdd;
    event Func<GroupManageEvent, Task> OnGroupMemberRemove;

    // Friend / C2C
    event Func<C2CManageEvent, Task> OnFriendAdd;
    event Func<C2CManageEvent, Task> OnFriendDel;
    event Func<C2CManageEvent, Task> OnC2CMsgReject;
    event Func<C2CManageEvent, Task> OnC2CMsgReceive;

    // Forum thread
    event Func<ThreadModel, Task> OnForumThreadCreate;
    event Func<ThreadModel, Task> OnForumThreadUpdate;
    event Func<ThreadModel, Task> OnForumThreadDelete;

    // Forum post / reply / audit  (raw dict payload)
    event Func<Dictionary<string, object?>, Task> OnForumPostCreate;
    event Func<Dictionary<string, object?>, Task> OnForumPostDelete;
    event Func<Dictionary<string, object?>, Task> OnForumReplyCreate;
    event Func<Dictionary<string, object?>, Task> OnForumReplyDelete;
    event Func<Dictionary<string, object?>, Task> OnForumPublishAuditResult;

    // Audio / live channel
    event Func<PublicAudio, Task> OnAudioOrLiveChannelMemberEnter;
    event Func<PublicAudio, Task> OnAudioOrLiveChannelMemberExit;

    // Open forum thread
    event Func<OpenThread, Task> OnOpenForumThreadCreate;
    event Func<OpenThread, Task> OnOpenForumThreadUpdate;
    event Func<OpenThread, Task> OnOpenForumThreadDelete;

    // Open forum post / reply  (raw dict payload)
    event Func<Dictionary<string, object?>, Task> OnOpenForumPostCreate;
    event Func<Dictionary<string, object?>, Task> OnOpenForumPostDelete;
    event Func<Dictionary<string, object?>, Task> OnOpenForumReplyCreate;
    event Func<Dictionary<string, object?>, Task> OnOpenForumReplyDelete;

    // ── Backward-compatible raw access ──
    void On(string eventName, Delegate handler);
    void Off(string eventName, Delegate handler);
    Task InvokeAsync(string eventName, params object?[] args);
}
