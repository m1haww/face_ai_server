using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using PhotoAiBackend.Services;
using System.Text.Json;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/runway/")]
public class RunwayController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IRunwayService _runwayService;
    private readonly INotificationService _notificationService;

    public RunwayController(AppDbContext dbContext, IRunwayService runwayService, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _runwayService = runwayService;
        _notificationService = notificationService;
    }

    [HttpPost("text-to-image")]
    public async Task<IActionResult> GenerateTextToImage([FromBody] RunwayTextToImagePayload payload)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId);
            if (user == null) return BadRequest("User not found");

            if (user.Credits < 10)
            {
                return BadRequest("Insufficient credits");
            }

            var request = new RunwayTextToImageRequest
            {
                PromptText = payload.PromptText,
                Ratio = payload.Ratio,
                Model = "gen4_image",
                Seed = payload.Seed,
                ReferenceImages = payload.ReferenceImages,
                ContentModeration = new RunwayContentModeration { PublicFigureThreshold = "auto" }
            };

            var taskResponse = await _runwayService.CreateTextToImageTaskAsync(request);

            var job = new ImageJob
            {
                Id = 0,
                CreationDate = DateTime.UtcNow,
                Status = JobStatus.Processing,
                SystemPrompt = payload.PromptText,
                UserId = payload.UserId,
                Images = "[]",
                PresetCategory = PresetCategory.RunwayGenerated
            };

            await _dbContext.ImageJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            var runwayJob = new RunwayImageJob
            {
                ImageJobId = job.Id,
                RunwayTaskId = taskResponse.Id,
                TaskType = "text_to_image",
                Status = "PENDING",
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();

            return Ok(new { jobId = job.Id, taskId = taskResponse.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("image-to-video")]
    public async Task<IActionResult> GenerateImageToVideo([FromBody] RunwayImageToVideoPayload payload)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId);
            if (user == null) return BadRequest("User not found");

            if (user.Credits < 25)
            {
                return BadRequest("Insufficient credits");
            }

            var request = new RunwayImageToVideoRequest
            {
                PromptImage = payload.PromptImage,
                Model = payload.Model,
                Ratio = payload.Ratio,
                PromptText = payload.PromptText,
                Duration = payload.Duration,
                Seed = payload.Seed
            };

            var taskResponse = await _runwayService.CreateImageToVideoTaskAsync(request);

            var job = new ImageJob
            {
                Id = 0,
                CreationDate = DateTime.UtcNow,
                Status = JobStatus.Processing,
                SystemPrompt = payload.PromptText ?? "Image to video generation",
                UserId = payload.UserId,
                Images = "[]",
                PresetCategory = PresetCategory.RunwayGenerated
            };

            await _dbContext.ImageJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            var runwayJob = new RunwayImageJob
            {
                ImageJobId = job.Id,
                RunwayTaskId = taskResponse.Id,
                TaskType = "image_to_video",
                Status = "PENDING",
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();

            return Ok(new { jobId = job.Id, taskId = taskResponse.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("video-upscale")]
    public async Task<IActionResult> UpscaleVideo([FromBody] RunwayVideoUpscalePayload payload)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId);
            if (user == null) return BadRequest("User not found");

            if (user.Credits < 20)
            {
                return BadRequest("Insufficient credits");
            }

            var request = new RunwayVideoUpscaleRequest
            {
                VideoUri = payload.VideoUri,
                Model = "upscale_v1"
            };

            var taskResponse = await _runwayService.CreateVideoUpscaleTaskAsync(request);

            var job = new ImageJob
            {
                Id = 0,
                CreationDate = DateTime.UtcNow,
                Status = JobStatus.Processing,
                SystemPrompt = "Video upscale",
                UserId = payload.UserId,
                Images = "[]",
                PresetCategory = PresetCategory.RunwayGenerated
            };

            await _dbContext.ImageJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            var runwayJob = new RunwayImageJob
            {
                ImageJobId = job.Id,
                RunwayTaskId = taskResponse.Id,
                TaskType = "video_upscale",
                Status = "PENDING",
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();

            return Ok(new { jobId = job.Id, taskId = taskResponse.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("task-status/{taskId}")]
    public async Task<IActionResult> GetTaskStatus(string taskId)
    {
        try
        {
            var taskResponse = await _runwayService.GetTaskStatusAsync(taskId);

            var runwayJob = await _dbContext.RunwayImageJobs
                .Include(r => r.ImageJob)
                .FirstOrDefaultAsync(r => r.RunwayTaskId == taskId);

            if (runwayJob == null)
            {
                return NotFound("Job not found");
            }

            runwayJob.Status = taskResponse.Status;
            runwayJob.UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow;

            if (taskResponse.Status == "SUCCEEDED" && taskResponse.Output != null && taskResponse.Output.Any())
            {
                runwayJob.ImageJob.Images = JsonSerializer.Serialize(taskResponse.Output);
                runwayJob.ImageJob.Status = JobStatus.Done;

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == runwayJob.ImageJob.UserId);
                if (user != null)
                {
                    var creditCost = runwayJob.TaskType switch
                    {
                        "text_to_image" => 10,
                        "image_to_video" => 25,
                        "video_upscale" => 20,
                        _ => 10
                    };
                    
                    user.Credits -= creditCost;

                    if (user.FcmTokenId != null)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", GenerationType.RunwayImage.ToString() },
                            { "jobId", runwayJob.ImageJobId.ToString() },
                            { "taskType", runwayJob.TaskType }
                        };

                        var notification = new NotificationInfo
                        {
                            Title = $"Runway {runwayJob.TaskType.Replace("_", " ")} Complete!",
                            Text = $"Your Runway AI generation is ready. Tap to view your results."
                        };

                        await _notificationService.SendNotificatino(user.FcmTokenId, notification, data);
                    }
                }
            }
            else if (taskResponse.Status == "FAILED")
            {
                runwayJob.ImageJob.Status = JobStatus.Failed;
                runwayJob.FailureReason = taskResponse.Failure;
                runwayJob.FailureCode = taskResponse.FailureCode;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                taskId = taskResponse.Id,
                status = taskResponse.Status,
                output = taskResponse.Output,
                failure = taskResponse.Failure,
                failureCode = taskResponse.FailureCode,
                taskType = runwayJob.TaskType
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("cancel-task/{taskId}")]
    public async Task<IActionResult> CancelTask(string taskId)
    {
        try
        {
            var success = await _runwayService.CancelOrDeleteTaskAsync(taskId);
            
            if (success)
            {
                var runwayJob = await _dbContext.RunwayImageJobs
                    .Include(r => r.ImageJob)
                    .FirstOrDefaultAsync(r => r.RunwayTaskId == taskId);

                if (runwayJob != null)
                {
                    runwayJob.Status = "CANCELLED";
                    runwayJob.ImageJob.Status = JobStatus.Failed;
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new { success, message = success ? "Task cancelled successfully" : "Failed to cancel task" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("organization")]
    public async Task<IActionResult> GetOrganizationInfo()
    {
        try
        {
            var orgInfo = await _runwayService.GetOrganizationInfoAsync();
            return Ok(orgInfo);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("poll-and-update/{taskId}")]
    public async Task<IActionResult> PollAndUpdateTask(string taskId)
    {
        try
        {
            await GetTaskStatus(taskId);
            
            return Ok("Task status updated");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}