using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PhotoAiBackend.Persistance.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnhanceStatus
{
    [EnumMember(Value = "starting")]
    Starting,
    [EnumMember(Value = "running")]
    Running,
    [EnumMember(Value = "succeeded")]
    Succeeded,
    [EnumMember(Value = "failed")]
    Failed,
    [EnumMember(Value = "canceled")]
    Canceled
}