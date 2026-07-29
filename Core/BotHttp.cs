using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using POPYBot.Types;

namespace POPYBot;

public class Route
{
    public string Method { get; }
    public string Path { get; }
    public bool IsSandbox { get; set; }

    private const string Domain = "api.sgroup.qq.com";
    private const string SandboxDomain = "sandbox.api.sgroup.qq.com";
    private const string Scheme = "https";

    private readonly Dictionary<string, string>? _parameters;

    public Route(string method, string path, bool isSandbox = false, Dictionary<string, string>? parameters = null)
    {
        Method = method;
        Path = path;
        IsSandbox = isSandbox;
        _parameters = parameters;
    }

    public string Url
    {
        get
        {
            var host = IsSandbox ? SandboxDomain : Domain;
            var url = $"{Scheme}://{host}{Path}";
            if (_parameters != null)
            {
                foreach (var kv in _parameters)
                    url = url.Replace($"{{{kv.Key}}}", kv.Value);
            }
            return url;
        }
    }
}

public class BotHttp : IDisposable
{
    private readonly HttpClient _httpClient;
    private Token? _token;
    private readonly int _timeout;
    private readonly bool _isSandbox;
    private bool _disposed;

    private static readonly HashSet<int> HttpOkStatus = new() { 200, 202, 204 };
    private const string XTpsTraceId = "X-Tps-trace-Id";

    public BotHttp(int timeout = 5, bool isSandbox = false)
    {
        _timeout = timeout;
        _isSandbox = isSandbox;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task EnsureSessionAsync()
    {
        if (_token == null)
            throw new InvalidOperationException("Token not set. Call LoginAsync first.");

        await _token.CheckTokenAsync();
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (_token?.AccessToken != null)
        {
            var authStr = _token.GetString();
            request.Headers.TryAddWithoutValidation("Authorization", authStr);
            request.Headers.TryAddWithoutValidation("X-Union-Appid", _token.AppId);
        }
    }

    public async Task<T?> RequestAsync<T>(Route route, object? json = null, int retryCount = 0)
    {
        if (retryCount > 2) return default;

        await EnsureSessionAsync();
        route.IsSandbox = _isSandbox;

        var url = route.Url;
        Logger.LogDebug($"[botpy] {route.Method} {url}");

        try
        {
            HttpRequestMessage request = new(new HttpMethod(route.Method), url);
            ApplyAuth(request);

            if (json != null)
            {
                var jsonStr = JsonSerializer.Serialize(json);
                request.Content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var traceId = response.Headers.TryGetValues(XTpsTraceId, out var vals)
                ? vals.FirstOrDefault() : null;

            string? responseBody = null;
            if (response.Content.Headers.ContentLength > 0)
                responseBody = await response.Content.ReadAsStringAsync();

            if (HttpOkStatus.Contains((int)response.StatusCode))
            {
                Logger.LogDebug($"[botpy] Request success: {url}, trace_id: {traceId}");
                if (string.IsNullOrEmpty(responseBody))
                    return default;

                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType == "application/json")
                    return JsonSerializer.Deserialize<T>(responseBody);
                return (T?)(object?)responseBody;
            }
            else
            {
                Logger.LogError($"[botpy] Request error: {url}, status: {(int)response.StatusCode}, body: {responseBody}, trace_id: {traceId}");

                if (Errors.HttpErrorDict.TryGetValue((int)response.StatusCode, out var errorType))
                {
                    var msg = TryGetMessage(responseBody);
                    throw (Exception?)Activator.CreateInstance(errorType, msg ?? "Unknown error")
                        ?? new ServerError("Unknown error");
                }

                throw new ServerError(TryGetMessage(responseBody) ?? "Unknown error");
            }
        }
        catch (TaskCanceledException)
        {
            Logger.LogWarning($"[botpy] Request timeout: {url}");
            return await RequestAsync<T>(route, json, retryCount + 1);
        }
        catch (HttpRequestException) when (retryCount < 2)
        {
            Logger.LogDebug("[botpy] Connection broken, retrying...");
            return await RequestAsync<T>(route, json, retryCount + 1);
        }
    }

    private static string? TryGetMessage(string? responseBody)
    {
        if (string.IsNullOrEmpty(responseBody)) return null;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : responseBody;
        }
        catch { return responseBody; }
    }

    public async Task<RobotInfo?> LoginAsync(Token token)
    {
        _token = token;
        await EnsureSessionAsync();
        return await RequestAsync<RobotInfo>(new Route("GET", "/users/@me"));
    }

    public async Task<WsApInfo?> GetWsUrlAsync()
    {
        return await RequestAsync<WsApInfo>(new Route("GET", "/gateway/bot"));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}
