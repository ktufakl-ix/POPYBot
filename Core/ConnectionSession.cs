namespace POPYBot;

using System.Text.Json;
using POPYBot.Models;
using POPYBot.Types;

public class ConnectionSession
{
    private readonly int _maxAsync;
    private readonly Func<Session, Task> _connect;
    private readonly Action<string, object?> _dispatch;
    private readonly object _lock = new();
    private readonly List<Session> _sessionList = new();

    public ConnectionState State { get; }

    public ConnectionSession(int maxAsync, Func<Session, Task> connect, Action<string, object?> dispatch, BotAPI api)
    {
        _maxAsync = maxAsync;
        _connect = connect;
        _dispatch = dispatch;
        State = new ConnectionState(dispatch, api);
    }

    public void Add(Session session)
    {
        lock (_lock)
        {
            _sessionList.Add(session);
            Logger.LogDebug($"[POPYBot] Session queued — sid: '{session.SessionId}', seq: {session.LastSeq}");
        }
    }

    public async Task MultiRunAsync(int sessionInterval = 5, CancellationToken ct = default)
    {
        List<Session> sessions;
        lock (_lock)
        {
            if (_sessionList.Count == 0) return;
            sessions = new List<Session>(_sessionList);
            _sessionList.Clear();
        }

        var tasks = new List<Task>();
        var index = 0;

        while (sessions.Count > 0 && !ct.IsCancellationRequested)
        {
            var batchSize = Math.Min(_maxAsync, sessions.Count);
            var timeInterval = sessionInterval * (index + 1);

            Logger.LogInfo($"[POPYBot] Max concurrency: {_maxAsync}, launching: {batchSize} session(s)");

            for (int i = 0; i < batchSize && sessions.Count > 0; i++)
            {
                var session = sessions[0];
                sessions.RemoveAt(0);
                tasks.Add(RunnerAsync(session, timeInterval));
            }
            index += _maxAsync;
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    private async Task RunnerAsync(Session session, int timeInterval)
    {
        await _connect(session);
        await Task.Delay(TimeSpan.FromSeconds(timeInterval));
    }
}

public class ConnectionState
{
    private readonly Action<string, object?> _dispatch;
    private readonly BotAPI _api;
    public Robot? Robot { get; set; }

    public Dictionary<string, Action<JsonElement>> Parsers { get; } = new();

    public ConnectionState(Action<string, object?> dispatch, BotAPI api)
    {
        _dispatch = dispatch;
        _api = api;
        RegisterParsers();
    }

    private void RegisterParsers()
    {
        Parsers["ready"] = (_) => _dispatch("ready", null);
        Parsers["resumed"] = (_) => _dispatch("resumed", null);

        Parsers["guild_create"] = (msg) => _dispatch("guild_create", new Guild(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["guild_update"] = (msg) => _dispatch("guild_update", new Guild(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["guild_delete"] = (msg) => _dispatch("guild_delete", new Guild(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["channel_create"] = (msg) => _dispatch("channel_create", new Channel(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["channel_update"] = (msg) => _dispatch("channel_update", new Channel(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["channel_delete"] = (msg) => _dispatch("channel_delete", new Channel(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["guild_member_add"] = (msg) => _dispatch("guild_member_add", new Member(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["guild_member_update"] = (msg) => _dispatch("guild_member_update", new Member(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["guild_member_remove"] = (msg) => _dispatch("guild_member_remove", new Member(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["message_create"] = (msg) =>
        {
            var payload = Deserialize<MessagePayload>(msg.GetProperty("d"));
            _dispatch("message_create", new Message(_api, msg.TryGetString("id"), payload));
        };
        Parsers["message_delete"] = (msg) =>
        {
            var payload = Deserialize<MessagePayload>(msg.GetProperty("d"));
            _dispatch("message_delete", new Message(_api, msg.TryGetString("id"), payload));
        };

        Parsers["message_reaction_add"] = (msg) => _dispatch("message_reaction_add", new Reaction(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["message_reaction_remove"] = (msg) => _dispatch("message_reaction_remove", new Reaction(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["direct_message_create"] = (msg) =>
        {
            var payload = Deserialize<DirectMessagePayload>(msg.GetProperty("d"));
            _dispatch("direct_message_create", new DirectMessage(_api, msg.TryGetString("id"), payload));
        };
        Parsers["direct_message_delete"] = (msg) =>
        {
            var payload = Deserialize<DirectMessagePayload>(msg.GetProperty("d"));
            _dispatch("direct_message_delete", new DirectMessage(_api, msg.TryGetString("id"), payload));
        };

        Parsers["interaction_create"] = (msg) => _dispatch("interaction_create", new Interaction(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["message_audit_pass"] = (msg) =>
        {
            var payload = Deserialize<MessageAuditPayload>(msg.GetProperty("d"));
            _dispatch("message_audit_pass", new MessageAudit(_api, msg.TryGetString("id"), payload));
        };
        Parsers["message_audit_reject"] = (msg) =>
        {
            var payload = Deserialize<MessageAuditPayload>(msg.GetProperty("d"));
            _dispatch("message_audit_reject", new MessageAudit(_api, msg.TryGetString("id"), payload));
        };

        Parsers["audio_start"] = (msg) => _dispatch("audio_start", new Audio(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["audio_finish"] = (msg) => _dispatch("audio_finish", new Audio(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["audio_on_mic"] = (msg) => _dispatch("audio_on_mic", new Audio(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["audio_off_mic"] = (msg) => _dispatch("audio_off_mic", new Audio(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["at_message_create"] = (msg) =>
        {
            var payload = Deserialize<MessagePayload>(msg.GetProperty("d"));
            _dispatch("at_message_create", new Message(_api, msg.TryGetString("id"), payload));
        };
        Parsers["public_message_delete"] = (msg) =>
        {
            var payload = Deserialize<MessagePayload>(msg.GetProperty("d"));
            _dispatch("public_message_delete", new Message(_api, msg.TryGetString("id"), payload));
        };

        Parsers["group_at_message_create"] = (msg) =>
        {
            var payload = Deserialize<GroupMessagePayload>(msg.GetProperty("d"));
            _dispatch("group_at_message_create", new GroupMessage(_api, msg.TryGetString("id"), payload));
        };
        Parsers["group_message_create"] = (msg) =>
        {
            var payload = Deserialize<GroupMessagePayload>(msg.GetProperty("d"));
            _dispatch("group_message_create", new GroupMessage(_api, msg.TryGetString("id"), payload));
        };
        Parsers["c2c_message_create"] = (msg) =>
        {
            var payload = Deserialize<MessagePayload>(msg.GetProperty("d"));
            _dispatch("c2c_message_create", new C2CMessage(_api, msg.TryGetString("id"), payload));
        };

        Parsers["group_add_robot"] = (msg) => _dispatch("group_add_robot", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["group_del_robot"] = (msg) => _dispatch("group_del_robot", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["group_msg_reject"] = (msg) => _dispatch("group_msg_reject", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["group_msg_receive"] = (msg) => _dispatch("group_msg_receive", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["group_member_add"] = (msg) => _dispatch("group_member_add", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["group_member_remove"] = (msg) => _dispatch("group_member_remove", new GroupManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["friend_add"] = (msg) => _dispatch("friend_add", new C2CManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["friend_del"] = (msg) => _dispatch("friend_del", new C2CManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["c2c_msg_reject"] = (msg) => _dispatch("c2c_msg_reject", new C2CManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["c2c_msg_receive"] = (msg) => _dispatch("c2c_msg_receive", new C2CManageEvent(_api, msg.TryGetString("id"), ParseData(msg)));

        Parsers["forum_thread_create"] = (msg) => _dispatch("forum_thread_create", new Thread(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["forum_thread_update"] = (msg) => _dispatch("forum_thread_update", new Thread(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["forum_thread_delete"] = (msg) => _dispatch("forum_thread_delete", new Thread(_api, msg.TryGetString("id"), ParseData(msg)));
        Parsers["forum_post_create"] = (msg) => _dispatch("forum_post_create", ParseData(msg));
        Parsers["forum_post_delete"] = (msg) => _dispatch("forum_post_delete", ParseData(msg));
        Parsers["forum_reply_create"] = (msg) => _dispatch("forum_reply_create", ParseData(msg));
        Parsers["forum_reply_delete"] = (msg) => _dispatch("forum_reply_delete", ParseData(msg));
        Parsers["forum_publish_audit_result"] = (msg) => _dispatch("forum_publish_audit_result", ParseData(msg));

        Parsers["audio_or_live_channel_member_enter"] = (msg) => _dispatch("audio_or_live_channel_member_enter", new PublicAudio(_api, ParseData(msg)));
        Parsers["audio_or_live_channel_member_exit"] = (msg) => _dispatch("audio_or_live_channel_member_exit", new PublicAudio(_api, ParseData(msg)));

        Parsers["open_forum_thread_create"] = (msg) => _dispatch("open_forum_thread_create", new OpenThread(_api, ParseData(msg)));
        Parsers["open_forum_thread_update"] = (msg) => _dispatch("open_forum_thread_update", new OpenThread(_api, ParseData(msg)));
        Parsers["open_forum_thread_delete"] = (msg) => _dispatch("open_forum_thread_delete", new OpenThread(_api, ParseData(msg)));
        Parsers["open_forum_post_create"] = (msg) => _dispatch("open_forum_post_create", ParseData(msg));
        Parsers["open_forum_post_delete"] = (msg) => _dispatch("open_forum_post_delete", ParseData(msg));
        Parsers["open_forum_reply_create"] = (msg) => _dispatch("open_forum_reply_create", ParseData(msg));
        Parsers["open_forum_reply_delete"] = (msg) => _dispatch("open_forum_reply_delete", ParseData(msg));
    }

    private static Dictionary<string, object?> ParseData(JsonElement msg)
    {
        if (msg.TryGetProperty("d", out var d))
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(d.GetRawText()) ?? new();
        }
        return new();
    }

    private static T Deserialize<T>(JsonElement element)
    {
        return JsonSerializer.Deserialize<T>(element.GetRawText())!;
    }
}

public static class JsonElementExtensions
{
    public static string? TryGetString(this JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }
}
