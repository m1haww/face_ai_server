using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class ImageGenerationCallbackPayload
{
    [JsonPropertyName("prompt")]
    public TunePromptResponse Prompt { get; set; }
}

public class TunePromptResponse
{
    [JsonPropertyName("negative_prompt")]
    public string NegativePrompt { get; set; }

    [JsonPropertyName("tune_id")]
    public int TuneId { get; set; }

    [JsonPropertyName("steps")]
    public int? Steps { get; set; }

    [JsonPropertyName("trained_at")]
    public DateTime TrainedAt { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("images")]
    public List<string> Images { get; set; }

    [JsonPropertyName("started_training_at")]
    public DateTime StartedTrainingAt { get; set; }
}