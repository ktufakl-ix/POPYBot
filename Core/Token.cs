namespace POPYBot;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Token
{
    private const string TokenUrl = "https://bots.qq.com/app/getAppAccessToken";
    private static readonly HttpClient _httpClient = new();

    public string AppId { get; }
    public string Secret { get; }
    public string? AccessToken { get; set; }
    public long ExpiresIn { get; set; }

    public Token(string appId, string secret)
    {
        AppId = appId;
        Secret = secret;
    }

    public string BotToken() => GetString();

    public string GetString()
    {
        if (AccessToken == null)
            throw new InvalidOperationException("Token not initialized");
        return $"QQBot {AccessToken}";
    }

    public async Task CheckTokenAsync()
    {
        if (AccessToken == null || DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= ExpiresIn)
        {
            await UpdateAccessTokenAsync();
        }
    }

    private async Task UpdateAccessTokenAsync()
    {
        Logger.LogInfo("[botpy] Updating access token...");

        var request = new
        {
            appId = AppId,
            clientSecret = Secret
        };

        HttpResponseMessage response;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            response = await _httpClient.PostAsJsonAsync(TokenUrl, request, cts.Token);
        }
        catch (TaskCanceledException)
        {
            Logger.LogError("[botpy] access_token request timeout");
            throw;
        }

        var data = await response.Content.ReadFromJsonAsync<TokenResponse>();

        if (data == null || string.IsNullOrEmpty(data.AccessToken))
        {
            Logger.LogError("[botpy] Failed to get token, check appid and secret");
            throw new InvalidOperationException("Failed to get access token");
        }

        Logger.LogInfo($"[botpy] access_token expires_in {data.ExpiresIn}");
        AccessToken = data.AccessToken;
        ExpiresIn = data.ExpiresIn + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        [JsonConverter(typeof(FlexibleLongConverter))]
        public long ExpiresIn { get; set; }
    }

    private class FlexibleLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetInt64(),
                JsonTokenType.String => long.TryParse(reader.GetString(), out var v) ? v : 0,
                _ => 0
            };
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
