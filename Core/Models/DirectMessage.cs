using POPYBot.Types;

namespace POPYBot.Models;

public class DirectMessage
{
    public BotAPI Api { get; }
    public MessageUser Author { get; }
    public string? ChannelId { get; }
    public string? Id { get; }
    public string? Content { get; }
    public bool DirectMessageFlag { get; }
    public string? GuildId { get; }
    public MessageMember? Member { get; }
    public MessageRef? MessageReference { get; }
    public List<Attachment> Attachments { get; }
    public int? Seq { get; }
    public int? MsgSeq { get; }
    public int? MessageType { get; }
    public List<MsgElement>? MsgElements { get; }
    public object? ArkData { get; }
    public string? SeqInChannel { get; }
    public string? SrcGuildId { get; }
    public string? Timestamp { get; }
    public string? EventId { get; }

    public DirectMessage(BotAPI api, string? eventId, DirectMessagePayload data)
    {
        Api = api;
        EventId = eventId;
        Author = new MessageUser(data.Author ?? new GatewayUser());
        ChannelId = data.ChannelId;
        Id = data.Id;
        Content = data.Content;
        DirectMessageFlag = data.DirectMessage;
        GuildId = data.GuildId;
        Member = data.Member != null ? new MessageMember(data.Member) : null;
        MessageReference = data.MessageReference;
        Attachments = data.Attachments?.Select(a => new Attachment(a)).ToList() ?? new();
        Seq = data.Seq;
        MsgSeq = data.MsgSeq;
        MessageType = data.MessageType;
        MsgElements = data.MsgElements;
        ArkData = data.ArkData?.Clone();
        SeqInChannel = data.SeqInChannel;
        SrcGuildId = data.SrcGuildId;
        Timestamp = data.Timestamp;
    }

    public async Task<object?> Reply(
        string? content = null,
        string? image = null,
        object? embed = null,
        object? ark = null,
        object? messageReference = null,
        string? imageUrl = null,
        object? markdown = null,
        object? keyboard = null)
    {
        return await Api.PostDmsAsync(GuildId ?? "", Id ?? "", content, image, embed, ark, messageReference, imageUrl, markdown, keyboard);
    }
}

public class MessageAudit
{
    public BotAPI Api { get; }
    public string? AuditId { get; }
    public string? MessageId { get; }
    public string? ChannelId { get; }
    public string? GuildId { get; }
    public string? EventId { get; }

    public MessageAudit(BotAPI api, string? eventId, MessageAuditPayload data)
    {
        Api = api;
        AuditId = data.AuditId;
        MessageId = data.MessageId;
        ChannelId = data.ChannelId;
        GuildId = data.GuildId;
        EventId = eventId;
    }
}

public class GroupMessage
{
    public BotAPI Api { get; }
    public string? Id { get; }
    public string? Content { get; }
    public MessageRef? MessageReference { get; }
    public List<MessageUser> Mentions { get; }
    public List<Attachment> Attachments { get; }
    public int? MsgSeq { get; }
    public int? MessageType { get; }
    public List<MsgElement>? MsgElements { get; }
    public object? ArkData { get; }
    public string? Timestamp { get; }
    public string? EventId { get; }
    public GroupMessageAuthor Author { get; }
    public string? GroupOpenid { get; }

    public GroupMessage(BotAPI api, string? eventId, GroupMessagePayload data)
    {
        Api = api;
        Id = data.Id;
        Content = data.Content;
        MessageReference = data.MessageReference;
        Mentions = data.Mentions?.Select(m => new MessageUser(m)).ToList() ?? new();
        Attachments = data.Attachments?.Select(a => new Attachment(a)).ToList() ?? new();
        MsgSeq = data.MsgSeq;
        MessageType = data.MessageType;
        MsgElements = data.MsgElements;
        ArkData = data.ArkData?.Clone();
        Timestamp = data.Timestamp;
        EventId = eventId;
        Author = new GroupMessageAuthor(data.Author ?? new GatewayUser());
        GroupOpenid = data.GroupOpenid;
    }

    public async Task<object?> Reply(
        string? content = null,
        string? image = null,
        object? embed = null,
        object? ark = null,
        object? messageReference = null,
        string? imageUrl = null,
        object? markdown = null,
        object? keyboard = null)
    {
        return await Api.PostGroupMessageAsync(GroupOpenid ?? "", Id ?? "", content, image, embed, ark, messageReference, imageUrl, markdown, keyboard);
    }
}

public class GroupMessageAuthor
{
    public string? MemberOpenid { get; }

    public GroupMessageAuthor(GatewayUser data)
    {
        MemberOpenid = data.Id;
    }
}

public class C2CMessage
{
    public BotAPI Api { get; }
    public string? Id { get; }
    public string? Content { get; }
    public MessageRef? MessageReference { get; }
    public List<MessageUser> Mentions { get; }
    public List<Attachment> Attachments { get; }
    public int? MsgSeq { get; }
    public int? MessageType { get; }
    public List<MsgElement>? MsgElements { get; }
    public object? ArkData { get; }
    public string? Timestamp { get; }
    public string? EventId { get; }
    public C2CMessageAuthor Author { get; }

    public C2CMessage(BotAPI api, string? eventId, MessagePayload data)
    {
        Api = api;
        Id = data.Id;
        Content = data.Content;
        MessageReference = data.MessageReference;
        Mentions = data.Mentions?.Select(m => new MessageUser(m)).ToList() ?? new();
        Attachments = data.Attachments?.Select(a => new Attachment(a)).ToList() ?? new();
        MsgSeq = data.MsgSeq;
        MessageType = data.MessageType;
        MsgElements = data.MsgElements;
        ArkData = data.ArkData?.Clone();
        Timestamp = data.Timestamp;
        EventId = eventId;
        Author = new C2CMessageAuthor(data.Author ?? new GatewayUser());
    }

    public async Task<object?> Reply(
        string? content = null,
        string? image = null,
        object? embed = null,
        object? ark = null,
        object? messageReference = null,
        string? imageUrl = null,
        object? markdown = null,
        object? keyboard = null)
    {
        return await Api.PostC2CMessageAsync(Author.UserOpenid ?? "", Id ?? "", content, image, embed, ark, messageReference, imageUrl, markdown, keyboard);
    }
}

public class C2CMessageAuthor
{
    public string? UserOpenid { get; }

    public C2CMessageAuthor(GatewayUser data)
    {
        UserOpenid = data.Id;
    }
}
