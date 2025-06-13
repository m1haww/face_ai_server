using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PhotoAiBackend.Persistance.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PresetCategory
{
    [EnumMember(Value = "Headshots")]
    Headshots,

    [EnumMember(Value = "Business Suit")]
    Business,

    [EnumMember(Value = "Cartoon Avatar")]
    Cartoon,

    [EnumMember(Value = "Rapper Style")]
    Rapper,

    [EnumMember(Value = "Surf Line")]
    Surfing,

    [EnumMember(Value = "Gym Mode")]
    Gym
}