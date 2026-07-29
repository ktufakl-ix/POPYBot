namespace POPYBot;

using System.Text.Json.Serialization;

public class RobotInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}

public class Robot
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public string? Avatar { get; set; }

    public Robot(RobotInfo data)
    {
        Update(data);
    }

    public void Update(RobotInfo data)
    {
        Name = data.Username;
        Id = data.Id;
        Avatar = data.Avatar;
    }
}
