using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class RunwayImageGenerationRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "gen4_image";

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1920:1080";

    [JsonPropertyName("prompt_text")]
    public string PromptText { get; set; } = string.Empty;

    [JsonPropertyName("reference_images")]
    public List<RunwayReferenceImage>? ReferenceImages { get; set; }
}

public class RunwayReferenceImage
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;
}

public class RunwayTaskResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("failure")]
    public string? Failure { get; set; }

    [JsonPropertyName("failure_code")]
    public string? FailureCode { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("output")]
    public List<string>? Output { get; set; }
}

public class RunwayImageGenerationPayload
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1920:1080";

    [JsonPropertyName("referenceImages")]
    public List<RunwayReferenceImage>? ReferenceImages { get; set; }
}