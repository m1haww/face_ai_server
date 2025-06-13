using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class ImageGenerationPayload
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("presetCategory")]
    public string PresetCategory { get; set; } = string.Empty;
}