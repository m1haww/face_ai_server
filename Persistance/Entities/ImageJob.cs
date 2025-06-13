using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoAiBackend.Persistance.Entities;

[Table("image-jobs")]
public class ImageJob
{
    [Column("id")]
    public int Id { get; set; }
    [Column("user-id")]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [Column("status")]
    public JobStatus Status { get; set; }
    [Column("system-prompt")]
    public string SystemPrompt { get; set; }
    [Column("creation-date")]
    public DateTime CreationDate { get; set; }
    [Column("images")]
    public string Images { get; set; }
    [Column("preset-category")]
    public PresetCategory PresetCategory { get; set; }
}