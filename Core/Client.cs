using POPYBot.Plugins;
using POPYBot.Types;
using Microsoft.Extensions.Logging;

namespace POPYBot;

public class Client : IAsyncDisposable
{
    public Intents Intents { get; }
    public BotAPI Api { get; private set; } = null!;
    public BotHttp Http { get; private set; }
    public Robot? Robot => _connection?.State.Robot;

    private PluginLoader? _pluginLoader;
    private PluginHookManager? _hookManager;
    private ConnectionSession? _connection;
    private WsApInfo? _wsAp;
    private Token? _token;
    private CancellationTokenSource? _cts;
    private bool _closed;
    private bool _retCoro;

    public Client(Intents intents, int timeout = 5, bool isSandbox = false)
    {
        Intents = intents;
        Http = new BotHttp(timeout, isSandbox);
        Api = new BotAPI(Http);
    }

    public async Task StartAsync(string appId, string secret, bool retCoro = false)
    {
        _retCoro = retCoro;
        _token = new Token(appId, secret);

        // Load plugins
        _hookManager = new PluginHookManager();
        var pluginContext = new BotPluginContext(Api, _hookManager);

        var cfg = ConfigManager.LoadOrDefault();
        _pluginLoader = new PluginLoader(cfg.PluginsPath);
        await _pluginLoader.LoadPluginsAsync(pluginContext);

        await LoginAsync(_token);
        await InitBotAsync(_token);
    }

    public void Run(string appId, string secret)
    {
        _cts = new CancellationTokenSource();
        var cfg = ConfigManager.LoadOrDefault();
        var appid = string.IsNullOrEmpty(cfg.AppId) ? appId : cfg.AppId;
        var sec = string.IsNullOrEmpty(cfg.Secret) ? secret : cfg.Secret;

        if (!string.IsNullOrEmpty(cfg.LogLevel) && Enum.TryParse<LogLevel>(cfg.LogLevel, true, out var level))
            Logger.SetLevel(level);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => _cts.Cancel();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; _cts.Cancel(); };

        try
        {
            StartAsync(appid, sec).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            Logger.LogInfo("[POPYBot] Service stopped by user.");
        }
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
            Logger.LogInfo("[POPYBot] Service stopped by user.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[POPYBot] Fatal error: {ex}");
            throw;
        }
    }

    private async Task LoginAsync(Token token)
    {
        Logger.LogInfo("[botpy] Logging in...");

        var user = await Http.LoginAsync(token);
        if (user == null)
            throw new InvalidOperationException("[botpy] Login failed - no user data returned");

        _wsAp = await Api.GetWsUrlAsync();
        if (_wsAp == null)
            throw new InvalidOperationException("[botpy] Failed to get WebSocket URL");

        _connection = new ConnectionSession(
            _wsAp.SessionStartLimit.MaxConcurrency,
            BotConnectAsync,
            WsDispatch,
            Api
        );

        _connection.State.Robot = new Robot(user);
    }

    private async Task InitBotAsync(Token token)
    {
        Logger.LogInfo("[botpy] Starting bot...");

        if (_wsAp!.Shards > _wsAp.SessionStartLimit.Remaining)
            throw new InvalidOperationException("[botpy] Exceeded session limit");

        var concurrency = _wsAp.SessionStartLimit.MaxConcurrency;
        var sessionInterval = (int)Math.Round(5.0 / concurrency);

        Logger.LogInfo($"[botpy] Gateway: {_wsAp.Url}, shards: {_wsAp.Shards}, concurrency: {concurrency}, intents: {(uint)Intents}");

        for (int i = 0; i < _wsAp.Shards; i++)
        {
            var session = new Session
            {
                SessionId = "",
                LastSeq = 0,
                Intent = (uint)Intents,
                Token = token,
                Url = _wsAp.Url,
                Shards = new ShardConfig { ShardId = i, ShardCount = _wsAp.Shards }
            };
            _connection!.Add(session);
            Logger.LogInfo($"[botpy] Added session {i + 1}/{_wsAp.Shards}");
        }

        while (!_closed)
        {
            try
            {
                await _connection!.MultiRunAsync(sessionInterval, _cts!.Token);

                // When MultiRunAsync returns, all sessions finished (disconnected).
                // Check if new sessions were re-added for reconnect.
                // Small delay to prevent tight spinning.
                await Task.Delay(500, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInfo("[POPYBot] Service force stopped!");
                break;
            }
        }

        await CloseAsync();
        Logger.LogInfo("[POPYBot] Service stopped.");
    }

    private async Task BotConnectAsync(Session session)
    {
        Logger.LogInfo("[botpy] Session starting...");
        var wsClient = new BotWebSocket(session, _connection!, _cts?.Token ?? default);
        try
        {
            await wsClient.WsConnectAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[botpy] BotConnect error: {ex.Message}");
        }
    }

    private void WsDispatch(string eventName, object? data)
    {
        Logger.LogDebug($"[botpy] Dispatching event: {eventName}");

        // Invoke plugin hooks
        _ = _hookManager?.InvokeAsync(eventName, data);

        // Invoke Client virtual methods
        var methodInfo = GetType().GetMethod("On" + eventName.Replace("_", "").Replace(" ", ""),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        if (methodInfo != null)
        {
            try
            {
                var parameters = data != null ? new[] { data } : null;
                var result = methodInfo.Invoke(this, parameters);
                if (result is Task task)
                    _ = task;
            }
            catch (Exception ex)
            {
                Logger.LogError($"[botpy] Event handler error for {eventName}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        else
        {
            Logger.LogDebug($"[botpy] Event {eventName} not registered");
        }
    }

    // ========== Virtual event handlers that users override ==========

    public virtual Task OnReady() => Task.CompletedTask;
    public virtual Task OnError(string eventMethod, Exception ex) => Task.CompletedTask;

    // Guild events
    public virtual Task OnGuildCreate(Models.Guild guild) => Task.CompletedTask;
    public virtual Task OnGuildUpdate(Models.Guild guild) => Task.CompletedTask;
    public virtual Task OnGuildDelete(Models.Guild guild) => Task.CompletedTask;
    public virtual Task OnChannelCreate(Models.Channel channel) => Task.CompletedTask;
    public virtual Task OnChannelUpdate(Models.Channel channel) => Task.CompletedTask;
    public virtual Task OnChannelDelete(Models.Channel channel) => Task.CompletedTask;

    // Member events
    public virtual Task OnGuildMemberAdd(Models.Member member) => Task.CompletedTask;
    public virtual Task OnGuildMemberUpdate(Models.Member member) => Task.CompletedTask;
    public virtual Task OnGuildMemberRemove(Models.Member member) => Task.CompletedTask;

    // Message events
    public virtual Task OnMessageCreate(Models.Message message) => Task.CompletedTask;
    public virtual Task OnMessageDelete(Models.Message message) => Task.CompletedTask;
    public virtual Task OnAtMessageCreate(Models.Message message) => Task.CompletedTask;
    public virtual Task OnPublicMessageDelete(Models.Message message) => Task.CompletedTask;

    // Direct message events
    public virtual Task OnDirectMessageCreate(Models.DirectMessage message) => Task.CompletedTask;
    public virtual Task OnDirectMessageDelete(Models.DirectMessage message) => Task.CompletedTask;

    // Reaction events
    public virtual Task OnMessageReactionAdd(Models.Reaction reaction) => Task.CompletedTask;
    public virtual Task OnMessageReactionRemove(Models.Reaction reaction) => Task.CompletedTask;

    // Interaction events
    public virtual Task OnInteractionCreate(Models.Interaction interaction) => Task.CompletedTask;

    // Message audit events
    public virtual Task OnMessageAuditPass(Models.MessageAudit audit) => Task.CompletedTask;
    public virtual Task OnMessageAuditReject(Models.MessageAudit audit) => Task.CompletedTask;

    // Audio events
    public virtual Task OnAudioStart(Models.Audio audio) => Task.CompletedTask;
    public virtual Task OnAudioFinish(Models.Audio audio) => Task.CompletedTask;
    public virtual Task OnMic(Models.Audio audio) => Task.CompletedTask;
    public virtual Task OffMic(Models.Audio audio) => Task.CompletedTask;

    // Public group/C2C events
    public virtual Task OnGroupAtMessageCreate(Models.GroupMessage message) => Task.CompletedTask;
    public virtual Task OnC2CMessageCreate(Models.C2CMessage message) => Task.CompletedTask;
    public virtual Task OnGroupAddRobot(Models.GroupManageEvent ev) => Task.CompletedTask;
    public virtual Task OnGroupDelRobot(Models.GroupManageEvent ev) => Task.CompletedTask;
    public virtual Task OnGroupMsgReject(Models.GroupManageEvent ev) => Task.CompletedTask;
    public virtual Task OnGroupMsgReceive(Models.GroupManageEvent ev) => Task.CompletedTask;
    public virtual Task OnFriendAdd(Models.C2CManageEvent ev) => Task.CompletedTask;
    public virtual Task OnFriendDel(Models.C2CManageEvent ev) => Task.CompletedTask;
    public virtual Task OnC2CMsgReject(Models.C2CManageEvent ev) => Task.CompletedTask;
    public virtual Task OnC2CMsgReceive(Models.C2CManageEvent ev) => Task.CompletedTask;

    // Forum events
    public virtual Task OnForumThreadCreate(Models.Thread thread) => Task.CompletedTask;
    public virtual Task OnForumThreadUpdate(Models.Thread thread) => Task.CompletedTask;
    public virtual Task OnForumThreadDelete(Models.Thread thread) => Task.CompletedTask;
    public virtual Task OnForumPostCreate(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnForumPostDelete(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnForumReplyCreate(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnForumReplyDelete(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnForumPublishAuditResult(Dictionary<string, object?> data) => Task.CompletedTask;

    // Audio/Live channel member events
    public virtual Task OnAudioOrLiveChannelMemberEnter(Models.PublicAudio audio) => Task.CompletedTask;
    public virtual Task OnAudioOrLiveChannelMemberExit(Models.PublicAudio audio) => Task.CompletedTask;

    // Open forum events
    public virtual Task OnOpenForumThreadCreate(Models.OpenThread thread) => Task.CompletedTask;
    public virtual Task OnOpenForumThreadUpdate(Models.OpenThread thread) => Task.CompletedTask;
    public virtual Task OnOpenForumThreadDelete(Models.OpenThread thread) => Task.CompletedTask;
    public virtual Task OnOpenForumPostCreate(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnOpenForumPostDelete(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnOpenForumReplyCreate(Dictionary<string, object?> data) => Task.CompletedTask;
    public virtual Task OnOpenForumReplyDelete(Dictionary<string, object?> data) => Task.CompletedTask;

    public async Task CloseAsync()
    {
        if (_closed) return;
        _closed = true;
        _cts?.Cancel();

        if (_pluginLoader != null)
            await _pluginLoader.ShutdownAllAsync();

        Http.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}

public class BotPluginContext : IPluginContext
{
    public BotAPI Api { get; }
    public IPluginHookManager Hooks { get; }

    public BotPluginContext(BotAPI api, IPluginHookManager hooks)
    {
        Api = api;
        Hooks = hooks;
    }
}
