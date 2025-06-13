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

    [HttpPost("generate-image")]
    public async Task<IActionResult> GenerateImage([FromBody] RunwayImageGenerationPayload payload)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId);
            if (user == null) return BadRequest("User not found");

            if (user.Credits < 15)
            {
                return BadRequest("Insufficient credits");
            }

            var request = new RunwayImageGenerationRequest
            {
                Model = "gen4_image",
                Ratio = payload.Ratio,
                PromptText = payload.Prompt,
                ReferenceImages = payload.ReferenceImages
            };

            var taskResponse = await _runwayService.CreateImageGenerationTaskAsync(request);

            var job = new ImageJob
            {
                Id = 0,
                CreationDate = DateTime.UtcNow,
                Status = JobStatus.Processing,
                SystemPrompt = payload.Prompt,
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
                Status = taskResponse.Status,
                CreatedAt = taskResponse.CreatedAt,
                UpdatedAt = taskResponse.UpdatedAt
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
            runwayJob.UpdatedAt = taskResponse.UpdatedAt;

            if (taskResponse.Status == "SUCCEEDED" && taskResponse.Output != null && taskResponse.Output.Any())
            {
                runwayJob.ImageJob.Images = JsonSerializer.Serialize(taskResponse.Output);
                runwayJob.ImageJob.Status = JobStatus.Done;

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == runwayJob.ImageJob.UserId);
                if (user != null)
                {
                    user.Credits -= 15;

                    if (user.FcmTokenId != null)
                    {
                        var data = new Dictionary<string, string>
                        {
                            { "type", GenerationType.RunwayImage.ToString() },
                            { "jobId", runwayJob.ImageJobId.ToString() }
                        };

                        var notification = new NotificationInfo
                        {
                            Title = "Image Generation Complete!",
                            Text = "Your Runway AI-generated image is ready. Tap to view your results."
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
                failureCode = taskResponse.FailureCode
            });
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