using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class TuneModelResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("face_crop")]
    public bool FaceCrop { get; set; }

    [JsonPropertyName("branch")]
    public string Branch { get; set; }

    [JsonPropertyName("base_tune_id")]
    public int BaseTuneId { get; set; }

    [JsonPropertyName("orig_images")]
    public List<string> OrigImages { get; set; }

    [JsonPropertyName("started_training_at")]
    public DateTime? StartedTrainingAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("ckpt_url")]
    public string CkptUrl { get; set; }

    [JsonPropertyName("eta")]
    public DateTime Eta { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("steps")]
    public int? Steps { get; set; }

    [JsonPropertyName("callback")]
    public string Callback { get; set; }

    [JsonPropertyName("ckpt_urls")]
    public List<string> CkptUrls { get; set; }

    [JsonPropertyName("model_type")]
    public string ModelType { get; set; }

    [JsonPropertyName("args")]
    public object Args { get; set; }

    [JsonPropertyName("is_api")]
    public bool IsApi { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("trained_at")]
    public DateTime? TrainedAt { get; set; }
}