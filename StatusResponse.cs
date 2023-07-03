using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MCStatus;

public record struct Player(string Name, [property: JsonPropertyName("id")] Guid Uuid);

/// <param name="Max">Maximum amount of concurrent players on the server</param>
/// <param name="Online">Current amount of players on the server</param>
/// <param name="Sample">May provide a few currently online players. Often empty</param>
public record struct Players(int Max, int Online, Player[] Sample);

/// <param name="Name">Specifies the Minecraft version and software the server is running on. E.g. "Spigot 1.20.1"</param>
/// <param name="Protocol">Protocol to be used for communicating with the server</param>
public record struct Version(string Name, int Protocol);

/// <param name="EnforcesSecureChat">Whether cryptographically signed chat messages are required</param>
/// <param name="PreviewsChat">See https://www.minecraft.net/en-us/article/minecraft-snapshot-22w19a</param>
public record struct StatusResponse(Players Players, Version Version, bool EnforcesSecureChat, bool PreviewsChat,
    [property: JsonIgnore] string Description, [property: JsonPropertyName("favicon")] string? FavIconBase64) {
    private static JsonSerializerOptions JsonOptions { get; } = new() {
        PropertyNameCaseInsensitive = true,
    };

    internal static StatusResponse Deserialize(string json) {
        var node = JsonNode.Parse(json);
        var ret = node.Deserialize<StatusResponse>(JsonOptions);
        var desc = node["description"];
        var descStr = desc is JsonValue ? desc.ToString() : (desc?["extra"]?[0]?["text"] ?? desc?["text"])?.ToString() ?? "";
        return ret with { Description = descStr,
            Players = ret.Players with { Sample = ret.Players.Sample ?? Array.Empty<Player>() } };
    }
}
