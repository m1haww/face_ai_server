using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class ImageGenerationResponse
{ 
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("callback")]
        public string Callback { get; set; }

        [JsonPropertyName("trained_at")]
        public DateTime? TrainedAt { get; set; }

        [JsonPropertyName("started_training_at")]
        public DateTime? StartedTrainingAt { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("tune_id")]
        public int TuneId { get; set; }

        [JsonPropertyName("prompt_likes_count")]
        public int PromptLikesCount { get; set; }

        [JsonPropertyName("base_pack_id")]
        public int? BasePackId { get; set; }

        [JsonPropertyName("input_image")]
        public string InputImage { get; set; }

        [JsonPropertyName("mask_image")]
        public string MaskImage { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("negative_prompt")]
        public string NegativePrompt { get; set; }

        [JsonPropertyName("cfg_scale")]
        public double? CfgScale { get; set; }

        [JsonPropertyName("steps")]
        public int? Steps { get; set; }

        [JsonPropertyName("super_resolution")]
        public bool SuperResolution { get; set; }

        [JsonPropertyName("ar")]
        public string Ar { get; set; }

        [JsonPropertyName("num_images")]
        public int NumImages { get; set; }

        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        [JsonPropertyName("controlnet_conditioning_scale")]
        public double? ControlnetConditioningScale { get; set; }

        [JsonPropertyName("controlnet_txt2img")]
        public bool ControlnetTxt2Img { get; set; }

        [JsonPropertyName("denoising_strength")]
        public double? DenoisingStrength { get; set; }

        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("w")]
        public int? W { get; set; }

        [JsonPropertyName("h")]
        public int? H { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();

        [JsonPropertyName("liked")]
        public bool Liked { get; set; }

        [JsonPropertyName("tune")]
        public Tune Tune { get; set; }

        [JsonPropertyName("tunes")]
        public List<Tune> Tunes { get; set; } = new();
}

public class Tune
{
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
}