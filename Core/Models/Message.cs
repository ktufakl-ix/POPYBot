using POPYBot.Types;

namespace POPYBot.Models;

public class Message
{
    public BotAPI Api { get; }
    public MessageUser Author { get; }
    public string? ChannelId { get; }
    public string? Id { get; }
    public string? Content { get; }
    public string? GuildId { get; }
    public MessageMember? Member { get; }
    public MessageRef? MessageReference { get; }
    public List<MessageUser> Mentions { get; }
    public List<Attachment> Attachments { get; }
    public int? Seq { get; }
    public int? MsgSeq { get; }
    public int? MessageType { get; }
    public List<MsgElement>? MsgElements { get; }
    public object? ArkData { get; }
    public string? SeqInChannel { get; }
    public string? Timestamp { get; }
    public string? EventId { get; }

    public Message(BotAPI api, string? eventId, MessagePayload data)
    {
        Api = api;
        EventId = eventId;
        Author = new MessageUser(data.Author ?? new GatewayUser());
        ChannelId = data.ChannelId;
        Id = data.Id;
        Content = data.Content;
        GuildId = data.GuildId;
        Member = data.Member != null ? new MessageMember(data.Member) : null;
        MessageReference = data.MessageReference;
        Mentions = data.Mentions?.Select(m => new MessageUser(m)).ToList() ?? new();
        Attachments = data.Attachments?.Select(a => new Attachment(a)).ToList() ?? new();
        Seq = data.Seq;
        MsgSeq = data.MsgSeq;
        MessageType = data.MessageType;
        MsgElements = data.MsgElements;
        ArkData = data.ArkData?.Clone();
        SeqInChannel = data.SeqInChannel;
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
        return await Api.PostMessageAsync(ChannelId ?? "", Id ?? "", content, image, embed, ark, messageReference, imageUrl, markdown, keyboard);
    }
}

public class MessageUser
{
    public string? Id { get; }
    public string? Username { get; }
    public bool? Bot { get; }
    public string? Avatar { get; }

    public MessageUser(GatewayUser data)
    {
        Id = data.Id;
        Username = data.Username;
        Bot = data.Bot;
        Avatar = data.Avatar;
    }
}

public class MessageMember
{
    public string? Nick { get; }
    public List<string>? Roles { get; }
    public string? JoinedAt { get; }

    public MessageMember(GatewayMember data)
    {
        Nick = data.Nick;
        Roles = data.Roles;
        JoinedAt = data.JoinedAt;
    }
}

public class Attachment
{
    public string? ContentType { get; }
    public string? Filename { get; }
    public int? Height { get; }
    public int? Width { get; }
    public string? Id { get; }
    public int? Size { get; }
    public string? Url { get; }

    public Attachment(MessageAttachment data)
    {
        ContentType = data.ContentType;
        Filename = data.Filename;
        Height = data.Height;
        Width = data.Width;
        Id = data.Id;
        Size = data.Size;
        Url = data.Url;
    }
}
