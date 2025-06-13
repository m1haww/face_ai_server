using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models
{
    public class EnhanceCallbackPayload
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("urls")]
        public PredictionUrls Urls { get; set; }

        [JsonPropertyName("output")]
        public string Output { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("metrics")]
        public PredictionMetrics Metrics { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("logs")]
        public string Logs { get; set; }

        [JsonPropertyName("data_removed")]
        public bool DataRemoved { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("webhook_events_filter")]
        public List<string> WebhookEventsFilter { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTime CompletedAt { get; set; }

        [JsonPropertyName("webhook")]
        public string Webhook { get; set; }

        [JsonPropertyName("input")]
        public EnhanceInput Input { get; set; }
    }

    public class PredictionUrls
    {
        [JsonPropertyName("get")]
        public string Get { get; set; }

        [JsonPropertyName("stream")]
        public string Stream { get; set; }

        [JsonPropertyName("cancel")]
        public string Cancel { get; set; }
    }

    public class PredictionMetrics
    {
        [JsonPropertyName("predict_time")]
        public double PredictTime { get; set; }
    }

    public class EnhanceInput
    {
        [JsonPropertyName("face_enhance")]
        public bool FaceEnhance { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }
}