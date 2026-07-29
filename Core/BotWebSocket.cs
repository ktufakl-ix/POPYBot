using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using POPYBot.Types;

namespace POPYBot;
using POPYBot.Types;

public class BotWebSocket
{
    private const int WsDispatchEvent = 0;
    private const int WsHeartbeat = 1;
    private const int WsIdentify = 2;
    private const int WsResume = 6;
    private const int WsReconnect = 7;
    private const int WsInvalidSession = 9;
    private const int WsHello = 10;
    private const int WsHeartbeatAck = 11;

    private readonly int[] InvalidReconnectCode = { 9001, 9005 };
    private readonly int[] AuthFailCode = { 4004, 4005 };

    private ClientWebSocket? _ws;
    private readonly Session _session;
    private readonly ConnectionSession _connection;
    private readonly Dictionary<string, Action<JsonElement>> _parser;
    private readonly CancellationToken _shutdownToken;
    private bool _canReconnect = true;
    private CancellationTokenSource? _cts;
    private int _heartbeatInterval = 30; // default, overridden by HELLO

    public BotWebSocket(Session session, ConnectionSession connection, CancellationToken shutdownToken = default)
    {
        _session = session;
        _connection = connection;
        _parser = connection.State.Parsers;
        _shutdownToken = shutdownToken;
    }

    public async Task WsConnectAsync()
    {
        var url = _session.Url;
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("[POPYBot] Session URL is empty");

        Logger.LogInfo($"[POPYBot] Connecting to gateway: {url}");
        _ws = new ClientWebSocket();
        _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownToken);

        try
        {
            Logger.LogInfo("[POPYBot] Attempting WebSocket handshake (30s timeout)...");
            using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, connectCts.Token);
            await _ws.ConnectAsync(new Uri(url), linkedCts.Token);
            Logger.LogInfo("[POPYBot] WebSocket connected, waiting for HELLO...");

            var buffer = new byte[8192];
            var messageBuffer = new List<byte>();
            while (_ws.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                using var receiveCts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                using var linkedReceive = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, receiveCts.Token);
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), linkedReceive.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

                    if (result.EndOfMessage)
                    {
                        var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        messageBuffer.Clear();
                        await OnMessageAsync(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Logger.LogInfo($"[botpy] WebSocket closed: {_ws.CloseStatus} {_ws.CloseStatusDescription}");
                    await OnClosedAsync((int)(_ws.CloseStatus ?? WebSocketCloseStatus.Empty), _ws.CloseStatusDescription ?? "");
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (!_shutdownToken.IsCancellationRequested)
        {
            Logger.LogWarning("[POPYBot] WebSocket operation timed out, reconnecting...");
            _connection.Add(_session);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("[POPYBot] WebSocket shutdown requested");
        }
        catch (WebSocketException ex)
        {
            Logger.LogError($"[POPYBot] WebSocket connection failed: {ex.Message} (inner: {ex.InnerException?.Message})");
            await OnErrorAsync(ex);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[POPYBot] WebSocket error: {ex.GetType().Name}: {ex.Message}");
            await OnErrorAsync(ex);
        }
        finally
        {
            if (_ws?.State == WebSocketState.Open)
            {
                try { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); } catch { }
            }
            _ws?.Dispose();
        }
    }

    private async Task OnMessageAsync(string rawMessage)
    {
        Logger.LogDebug($"[botpy] Received: {rawMessage}");
        using var doc = JsonDocument.Parse(rawMessage);
        var msg = doc.RootElement;

        if (await IsSystemEventAsync(msg))
            return;

        var opcode = msg.TryGetProperty("op", out var op) ? op.GetInt32() : -1;
        var eventName = msg.TryGetProperty("t", out var t) ? t.GetString() ?? "" : "";
        var seq = msg.TryGetProperty("s", out var s) ? s.GetInt64() : 0;

        if (seq > 0)
            _session.LastSeq = seq;

        if (eventName == "READY")
        {
            var readyData = msg.GetProperty("d");
            ReadyHandler(readyData);
            Logger.LogInfo($"[botpy] Robot '{readyData.GetProperty("user").GetProperty("username").GetString()}' started!");
            _ = SendHeartAsync(_heartbeatInterval);
        }

        if (eventName == "RESUMED")
        {
            Logger.LogInfo("[botpy] Robot reconnected!");
            _ = SendHeartAsync(_heartbeatInterval);
        }

        if (!string.IsNullOrEmpty(eventName) && opcode == WsDispatchEvent)
        {
            var lowerEvent = eventName.ToLower();
            if (_parser.TryGetValue(lowerEvent, out var handler))
            {
                handler(msg);
            }
            else
            {
                Logger.LogDebug($"[botpy] Unknown event: {lowerEvent}");
            }
        }
    }

    private void ReadyHandler(JsonElement data)
    {
        _session.SessionId = data.GetProperty("session_id").GetString() ?? "";
        var shard = data.GetProperty("shard");
        _session.Shards.ShardId = shard[0].GetInt32();
        _session.Shards.ShardCount = shard[1].GetInt32();
    }

    private async Task<bool> IsSystemEventAsync(JsonElement msg)
    {
        if (!msg.TryGetProperty("op", out var opProp))
            return false;

        var op = opProp.GetInt32();

        switch (op)
        {
            case WsHello:
                // Read heartbeat_interval from HELLO (in milliseconds)
                if (msg.TryGetProperty("d", out var d) && d.TryGetProperty("heartbeat_interval", out var hi))
                {
                    _heartbeatInterval = (int)(hi.GetInt64() / 1000);
                    Logger.LogInfo($"[botpy] Server heartbeat interval: {_heartbeatInterval}s");
                }
                await OnConnectedAsync();
                return true;
            case WsHeartbeatAck:
                return true;
            case WsReconnect:
                _canReconnect = true;
                return true;
            case WsInvalidSession:
                _canReconnect = false;
                return true;
        }
        return false;
    }

    private async Task OnConnectedAsync()
    {
        if (_session.SessionId != "")
            await WsResumeAsync();
        else
            await WsIdentifyAsync();
    }

    private async Task WsIdentifyAsync()
    {
        await _session.Token!.CheckTokenAsync();

        var payload = new
        {
            op = WsIdentify,
            d = new
            {
                shard = new[] { _session.Shards.ShardId, _session.Shards.ShardCount },
                token = _session.Token.GetString(),
                intents = _session.Intent
            }
        };

        await SendAsync(JsonSerializer.Serialize(payload));
        Logger.LogInfo($"[botpy] Identify sent — shard: [{_session.Shards.ShardId}/{_session.Shards.ShardCount}], intents: {_session.Intent}");
    }

    private async Task WsResumeAsync()
    {
        await _session.Token!.CheckTokenAsync();
        Logger.LogInfo("[botpy] Resuming...");

        var payload = new
        {
            op = WsResume,
            d = new
            {
                token = _session.Token.GetString(),
                session_id = _session.SessionId,
                seq = _session.LastSeq
            }
        };

        await SendAsync(JsonSerializer.Serialize(payload));
    }

    private async Task SendAsync(string message)
    {
        Logger.LogDebug($"[botpy] Sending: {message}");
        if (_ws?.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
        }
    }

    private async Task SendHeartAsync(int interval)
    {
        Logger.LogInfo("[botpy] Heartbeat started...");
        var cts = _cts;
        while (_ws?.State == WebSocketState.Open && cts is { IsCancellationRequested: false })
        {
            var payload = new { op = WsHeartbeat, d = _session.LastSeq };
            await SendAsync(JsonSerializer.Serialize(payload));
            try { await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token); }
            catch (OperationCanceledException) { break; }
        }
        Logger.LogDebug("[botpy] Heartbeat stopped");
    }

    private async Task OnErrorAsync(Exception ex)
    {
        Logger.LogError($"[botpy] WebSocket error: {ex.GetType().Name}: {ex.Message}");
        // Wait a bit before retry to avoid hammering the server
        await Task.Delay(2000);
        _connection.Add(_session);
    }

    private async Task OnClosedAsync(int closeCode, string closeMsg)
    {
        Logger.LogInfo($"[botpy] Connection closed — code: {closeCode}, reason: {closeMsg}");

        if (AuthFailCode.Contains(closeCode))
        {
            Logger.LogWarning("[botpy] Authentication failed! Resetting token... Check your AppId and Secret.");
            _session.Token!.AccessToken = null;
            _session.SessionId = "";
            _session.LastSeq = 0;
        }

        if (InvalidReconnectCode.Contains(closeCode) || !_canReconnect)
        {
            Logger.LogInfo("[botpy] Cannot resume, creating fresh session");
            _session.SessionId = "";
            _session.LastSeq = 0;
        }

        _connection.Add(_session);
    }
}
