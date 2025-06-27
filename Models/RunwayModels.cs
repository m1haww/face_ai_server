using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

// Text to Image Models
public class RunwayTextToImageRequest
{
    [JsonPropertyName("promptText")]
    public string PromptText { get; set; } = string.Empty;

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1920:1080";

    [JsonPropertyName("model")]
    public string Model { get; set; } = "gen4_image";
    
    [JsonPropertyName("referenceImages")]
    public List<RunwayReferenceImage>? ReferenceImages { get; set; }

    [JsonPropertyName("contentModeration")]
    public RunwayContentModeration? ContentModeration { get; set; }
}

// Image to Video Models
public class RunwayImageToVideoRequest
{
    [JsonPropertyName("promptImage")]
    public object PromptImage { get; set; } = string.Empty; // Can be string or array

    [JsonPropertyName("model")]
    public string Model { get; set; } = "gen4_turbo";

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1280:720";

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("promptText")]
    public string? PromptText { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 10;
}

public class RunwayVideoUpscaleRequest
{
    [JsonPropertyName("videoUri")]
    public string VideoUri { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = "upscale_v1";
}

public class RunwayReferenceImage
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }
}

public class RunwayPromptImage
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public string Position { get; set; } = "first";
}

public class RunwayContentModeration
{
    [JsonPropertyName("publicFigureThreshold")]
    public string PublicFigureThreshold { get; set; } = "auto";
}

public class RunwayTaskResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("output")]
    public List<string>? Output { get; set; }

    [JsonPropertyName("failure")]
    public string? Failure { get; set; }

    [JsonPropertyName("failureCode")]
    public string? FailureCode { get; set; }
}

public class RunwayOrganizationResponse
{
    [JsonPropertyName("creditBalance")]
    public int CreditBalance { get; set; }

    [JsonPropertyName("tier")]
    public object Tier { get; set; } = new();

    [JsonPropertyName("usage")]
    public object Usage { get; set; } = new();
}

// Payload Models for API endpoints
public class RunwayTextToImagePayload
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("promptText")]
    public string PromptText { get; set; } = string.Empty;

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1920:1080";

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("referenceImages")]
    public List<RunwayReferenceImage>? ReferenceImages { get; set; }
}

public class RunwayImageToVideoPayload
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("promptImage")]
    public string PromptImage { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = "gen4_turbo";

    [JsonPropertyName("ratio")]
    public string Ratio { get; set; } = "1280:720";

    [JsonPropertyName("promptText")]
    public string? PromptText { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; } = 5;

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }
}

public class RunwayVideoUpscalePayload
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("videoUri")]
    public string VideoUri { get; set; } = string.Empty;
}