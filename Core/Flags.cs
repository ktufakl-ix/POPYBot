namespace POPYBot;

[Flags]
public enum Intents : uint
{
    None = 0,

    /// <summary>频道事件: guild_create, guild_update, guild_delete, channel_create, channel_update, channel_delete</summary>
    Guilds = 1 << 0,

    /// <summary>频道成员事件: guild_member_add, guild_member_update, guild_member_remove</summary>
    GuildMembers = 1 << 1,

    /// <summary>消息事件(仅私域): message_create, message_delete</summary>
    GuildMessages = 1 << 9,

    /// <summary>消息表态事件: message_reaction_add, message_reaction_remove</summary>
    GuildMessageReactions = 1 << 10,

    /// <summary>私信事件: direct_message_create, direct_message_delete</summary>
    DirectMessage = 1 << 12,

    /// <summary>音视频/直播子频道成员进出事件</summary>
    AudioOrLiveChannelMember = 1 << 19,

    /// <summary>开放论坛事件: open_forum_thread_*</summary>
    OpenForumEvent = 1 << 18,

    /// <summary>互动事件: interaction_create</summary>
    Interaction = 1 << 26,

    /// <summary>消息审核事件: message_audit_pass, message_audit_reject</summary>
    MessageAudit = 1 << 27,

    /// <summary>论坛事件(仅私域): forum_thread_*, forum_post_*, forum_reply_*, forum_publish_audit_result</summary>
    Forums = 1 << 28,

    /// <summary>音频事件: audio_start, audio_finish, on_mic, off_mic</summary>
    AudioAction = 1 << 29,

    /// <summary>公域消息事件: at_message_create, public_message_delete</summary>
    PublicGuildMessages = 1 << 30,

    /// <summary>群/C2C公域消息事件</summary>
    PublicMessages = 1 << 25,

    All = Guilds | GuildMembers | GuildMessages | GuildMessageReactions | DirectMessage |
          AudioOrLiveChannelMember | OpenForumEvent | Interaction | MessageAudit | Forums |
          AudioAction | PublicGuildMessages | PublicMessages,

    Default = All & ~GuildMessages & ~Forums,
}

[Flags]
public enum Permission : uint
{
    ViewPermission = 1 << 0,
    ManagerPermission = 1 << 1,
    SpeakPermission = 1 << 2,
    LivePermission = 1 << 3,
}
