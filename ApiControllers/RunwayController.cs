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
    private readonly IRunwayPollingService _pollingService;

    public RunwayController(AppDbContext dbContext, IRunwayService runwayService, INotificationService notificationService, IRunwayPollingService pollingService)
    {
        _dbContext = dbContext;
        _runwayService = runwayService;
        _notificationService = notificationService;
        _pollingService = pollingService;
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
                ReferenceImages = payload.ReferenceImages,
                ContentModeration = new RunwayContentModeration { PublicFigureThreshold = "auto" }
            };

            var taskResponse = await _runwayService.CreateTextToImageTaskAsync(request);

            var runwayJob = new RunwayImageJob
            {
                UserId = payload.UserId,
                RunwayTaskId = taskResponse.Id,
                TaskType = "text_to_image",
                Prompt = payload.PromptText,
                Status = "PENDING",
                OutputUrls = "[]",
                CreditCost = 10,
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();
            
            _pollingService.AddTask(taskResponse.Id);

            return Ok(new { jobId = runwayJob.Id, taskId = taskResponse.Id });
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

            if (user.Credits < 50)
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
            };

            var taskResponse = await _runwayService.CreateImageToVideoTaskAsync(request);

            var runwayJob = new RunwayImageJob
            {
                UserId = payload.UserId,
                RunwayTaskId = taskResponse.Id,
                TaskType = "image_to_video",
                Prompt = payload.PromptText ?? "Image to video generation",
                Status = "PENDING",
                OutputUrls = "[]",
                CreditCost = 50,
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();

            _pollingService.AddTask(taskResponse.Id);

            return Ok(new { jobId = runwayJob.Id, taskId = taskResponse.Id });
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

            var runwayJob = new RunwayImageJob
            {
                UserId = payload.UserId,
                RunwayTaskId = taskResponse.Id,
                TaskType = "video_upscale",
                Prompt = "Video upscale",
                Status = "PENDING",
                OutputUrls = "[]",
                CreditCost = 20,
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow
            };

            await _dbContext.RunwayImageJobs.AddAsync(runwayJob);
            await _dbContext.SaveChangesAsync();

            _pollingService.AddTask(taskResponse.Id);

            return Ok(new { jobId = runwayJob.Id, taskId = taskResponse.Id });
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
                .FirstOrDefaultAsync(r => r.RunwayTaskId == taskId);

            if (runwayJob == null)
            {
                return NotFound("Job not found");
            }

            runwayJob.Status = taskResponse.Status;
            runwayJob.UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow;

            if (taskResponse.Status == "SUCCEEDED" && taskResponse.Output != null && taskResponse.Output.Any())
            {
                runwayJob.OutputUrls = JsonSerializer.Serialize(taskResponse.Output);

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == runwayJob.UserId);
                if (user != null)
                {
                    user.Credits -= runwayJob.CreditCost;

                    if (user.FcmTokenId != null)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", GenerationType.RunwayImage.ToString() },
                            { "jobId", runwayJob.Id.ToString() },
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
                runwayJob.FailureReason = taskResponse.Failure;
                runwayJob.FailureCode = taskResponse.FailureCode;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                jobId = runwayJob.Id,
                taskId = taskResponse.Id,
                status = taskResponse.Status,
                output = taskResponse.Output,
                failure = taskResponse.Failure,
                failureCode = taskResponse.FailureCode,
                taskType = runwayJob.TaskType,
                prompt = runwayJob.Prompt
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetRunwayJob(int jobId)
    {
        try
        {
            var runwayJob = await _dbContext.RunwayImageJobs
                .FirstOrDefaultAsync(r => r.Id == jobId);

            if (runwayJob == null)
            {
                return NotFound("Job not found");
            }

            var result = new
            {
                jobId = runwayJob.Id,
                taskId = runwayJob.RunwayTaskId,
                taskType = runwayJob.TaskType,
                status = runwayJob.Status,
                prompt = runwayJob.Prompt,
                createdAt = runwayJob.CreatedAt,
                updatedAt = runwayJob.UpdatedAt,
                outputs = runwayJob.OutputUrls,
                creditCost = runwayJob.CreditCost,
                failureReason = runwayJob.FailureReason,
                userId = runwayJob.UserId
            };

            return Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}