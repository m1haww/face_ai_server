using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoAiBackend.Persistance.Entities;

[Table("runway-image-jobs")]
public class RunwayImageJob
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("user-id")]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    [Column("runway-task-id")]
    public string RunwayTaskId { get; set; }
    
    [Column("task-type")]
    public string TaskType { get; set; } = string.Empty;
    
    [Column("prompt")]
    public string Prompt { get; set; } = string.Empty;
    
    [Column("status")]
    public string Status { get; set; }
    
    [Column("output-urls")]
    public string OutputUrls { get; set; } = "[]";
    
    [Column("credit-cost")]
    public int CreditCost { get; set; }
    
    [Column("failure-reason")]
    public string? FailureReason { get; set; }
    
    [Column("failure-code")]
    public string? FailureCode { get; set; }
    
    [Column("created-at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated-at")]
    public DateTime UpdatedAt { get; set; }
    
    // Keep ImageJobId as nullable for backward compatibility during migration
    [Column("image-job-id")]
    public int? ImageJobId { get; set; }
}