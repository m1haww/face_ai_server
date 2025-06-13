using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoAiBackend.Persistance.Entities;

[Table("users")]
public class User
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("tune-id")]
    public int TuneId { get; set; }
    [Column("fcm-token-id")]
    public string? FcmTokenId { get; set; }
    [Column("gender")]
    public string Gender { get; set; }
    [Column("credits")]
    public int Credits { get; set; }
}