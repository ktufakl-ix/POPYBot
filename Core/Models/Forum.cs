namespace POPYBot.Models;

using System.Text.Json;

public class Thread
{
    public BotAPI Api { get; }
    public ThreadInfo? Info { get; }
    public string? ChannelId { get; }
    public string? GuildId { get; }
    public string? AuthorId { get; }
    public string? EventId { get; }

    public Thread(BotAPI api, string? eventId, Dictionary<string, object?> data)
    {
        Api = api;
        EventId = eventId;
        AuthorId = data.GetValueOrDefault("author_id")?.ToString();
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();

        if (data.TryGetValue("thread_info", out var t) && t is Dictionary<string, object?> threadData)
        {
            Info = new ThreadInfo(threadData);
        }
    }
}

public class OpenThread
{
    public BotAPI Api { get; }
    public string? GuildId { get; }
    public string? ChannelId { get; }
    public string? AuthorId { get; }

    public OpenThread(BotAPI api, Dictionary<string, object?> data)
    {
        Api = api;
        GuildId = data.GetValueOrDefault("guild_id")?.ToString();
        ChannelId = data.GetValueOrDefault("channel_id")?.ToString();
        AuthorId = data.GetValueOrDefault("author_id")?.ToString();
    }
}

public class ThreadInfo
{
    public ThreadTitle? Title { get; }
    public ThreadContent? Content { get; }
    public string? ThreadId { get; }
    public string? DateTime { get; }

    public ThreadInfo(Dictionary<string, object?> data)
    {
        ThreadId = data.GetValueOrDefault("thread_id")?.ToString();
        DateTime = data.GetValueOrDefault("date_time")?.ToString();

        var titleJson = data.GetValueOrDefault("title")?.ToString() ?? "{}";
        var contentJson = data.GetValueOrDefault("content")?.ToString() ?? "{}";

        try
        {
            Title = JsonSerializer.Deserialize<ThreadTitle>(titleJson);
            Content = JsonSerializer.Deserialize<ThreadContent>(contentJson);
        }
        catch { }
    }
}

public class ThreadTitle
{
    public List<ThreadParagraph>? Paragraphs { get; set; }
}

public class ThreadContent
{
    public List<ThreadParagraph>? Paragraphs { get; set; }
}

public class ThreadParagraph
{
    public List<ThreadElem>? Elems { get; set; }
    public object? Props { get; set; }
}

public class ThreadElem
{
    public int Type { get; set; }
    public ThreadText? Text { get; set; }
    public ThreadImage? Image { get; set; }
    public ThreadVideo? Video { get; set; }
    public ThreadUrl? Url { get; set; }
}

public class ThreadText
{
    public string? Text { get; set; }
}

public class ThreadImage
{
    public ThreadPlatImage? PlatImage { get; set; }
}

public class ThreadPlatImage
{
    public string? Url { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ImageId { get; set; }
}

public class ThreadVideo
{
    public ThreadPlatVideo? PlatVideo { get; set; }
}

public class ThreadPlatVideo
{
    public string? Url { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? VideoId { get; set; }
}

public class ThreadUrl
{
    public string? Url { get; set; }
    public string? Desc { get; set; }
}
