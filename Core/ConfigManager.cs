namespace POPYBot;

using System.Text.Json.Serialization;

public class ConfigManager
{
    public string AppId { get; set; } = "";
    public string Secret { get; set; } = "";
    public bool IsSandbox { get; set; } = false;
    public int Timeout { get; set; } = 5;
    public string? LogLevel { get; set; }

    [JsonConverter(typeof(IntentsJsonConverter))]
    public Intents? Intents { get; set; }

    public string? PluginsPath { get; set; }
    public Dictionary<string, object>? Extra { get; set; }

    public static ConfigManager Load(string? path = null)
    {
        path ??= Path.Combine(AppContext.BaseDirectory, "config.json");
        return TryLoad(path);
    }

    public static ConfigManager LoadOrDefault()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "config.json");
        return TryLoad(path);
    }

    private static ConfigManager TryLoad(string path)
    {
        if (!File.Exists(path))
        {
            Logger.LogWarning($"[POPYBot] config.json not found at {path}, using defaults");
            return new ConfigManager();
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = System.Text.Json.JsonSerializer.Deserialize<ConfigManager>(json);
            return config ?? new ConfigManager();
        }
        catch (Exception ex)
        {
            Logger.LogError($"[POPYBot] Failed to parse config.json: {ex.Message}");
            return new ConfigManager();
        }
    }
}

public class IntentsJsonConverter : System.Text.Json.Serialization.JsonConverter<Intents>
{
    public override Intents Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.Number)
        {
            return (Intents)reader.GetInt64();
        }

        if (reader.TokenType == System.Text.Json.JsonTokenType.String)
        {
            var str = reader.GetString() ?? "";
            Intents result = 0;
            foreach (var part in str.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse<Intents>(part.Trim(), true, out var intent))
                    result |= intent;
            }
            return result;
        }

        return 0;
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, Intents value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
