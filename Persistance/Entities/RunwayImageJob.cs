using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoAiBackend.Persistance.Entities;

[Table("runway-image-jobs")]
public class RunwayImageJob
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("image-job-id")]
    public int ImageJobId { get; set; }
    
    public ImageJob ImageJob { get; set; }
    
    [Column("runway-task-id")]
    public string RunwayTaskId { get; set; }
    
    [Column("status")]
    public string Status { get; set; }
    
    [Column("failure-reason")]
    public string? FailureReason { get; set; }
    
    [Column("failure-code")]
    public string? FailureCode { get; set; }
    
    [Column("created-at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated-at")]
    public DateTime UpdatedAt { get; set; }
}