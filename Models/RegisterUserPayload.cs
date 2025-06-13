using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class RegisterUserPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("tuneId")]
    public int? TuneId { get; set; }
    
    [JsonPropertyName("gender")]
    public string Gender { get; set; }
    
    [JsonPropertyName("fcmTokenId")]
    public string? FcmTokenId { get; set; }
}