using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PhotoAiBackend.Services;

public class RunwayPollingService : BackgroundService, IRunwayPollingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RunwayPollingService> _logger;
    private readonly ConcurrentDictionary<string, PollingTask> _activeTasks = new();
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(30);
    private readonly int _maxRetries = 120;

    public RunwayPollingService(IServiceProvider serviceProvider, ILogger<RunwayPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void AddTask(string taskId)
    {
        var pollingTask = new PollingTask
        {
            TaskId = taskId,
            NextPollTime = DateTime.UtcNow.Add(_initialDelay),
            CurrentDelay = _initialDelay,
            RetryCount = 0
        };

        _activeTasks.TryAdd(taskId, pollingTask);
        _logger.LogInformation($"Added Runway task {taskId} to polling queue");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the application to start
        await Task.Delay(5000, stoppingToken);

        // Load pending tasks from database on startup
        await LoadPendingTasksAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var tasksToCheck = _activeTasks.Values
                .Where(t => t.NextPollTime <= DateTime.UtcNow)
                .ToList();

            if (tasksToCheck.Any())
            {
                var tasks = tasksToCheck.Select(task => PollTaskAsync(task, stoppingToken));
                await Task.WhenAll(tasks);
            }

            await Task.Delay(1000, stoppingToken); // Check every second
        }
    }

    private async Task LoadPendingTasksAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingJobs = await dbContext.RunwayImageJobs
                .Where(r => r.Status == "PENDING" || r.Status == "RUNNING" || r.Status == "THROTTLED")
                .ToListAsync(cancellationToken);

            foreach (var job in pendingJobs)
            {
                AddTask(job.RunwayTaskId);
            }

            _logger.LogInformation($"Loaded {pendingJobs.Count} pending Runway tasks on startup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending Runway tasks");
        }
    }

    private async Task PollTaskAsync(PollingTask pollingTask, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var runwayService = scope.ServiceProvider.GetRequiredService<IRunwayService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var taskResponse = await runwayService.GetTaskStatusAsync(pollingTask.TaskId);

            var runwayJob = await dbContext.RunwayImageJobs
                .FirstOrDefaultAsync(r => r.RunwayTaskId == pollingTask.TaskId, cancellationToken);

            if (runwayJob == null)
            {
                _logger.LogWarning($"Runway job not found for task {pollingTask.TaskId}, removing from queue");
                _activeTasks.TryRemove(pollingTask.TaskId, out _);
                return;
            }

            runwayJob.Status = taskResponse.Status;
            runwayJob.UpdatedAt = taskResponse.UpdatedAt ?? DateTime.UtcNow;

            if (taskResponse.Status == "SUCCEEDED" && taskResponse.Output != null && taskResponse.Output.Any())
            {
                runwayJob.OutputUrls = JsonSerializer.Serialize(taskResponse.Output);

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == runwayJob.UserId, cancellationToken);
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
                            Title = GetNotificationTitle(runwayJob.TaskType),
                            Text = "Your Runway AI generation is ready. Tap to view your results."
                        };

                        await notificationService.SendNotificatino(user.FcmTokenId, notification, data);
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                _activeTasks.TryRemove(pollingTask.TaskId, out _);
                _logger.LogInformation($"Runway task {pollingTask.TaskId} completed successfully");
            }
            else if (taskResponse.Status == "FAILED")
            {
                runwayJob.FailureReason = taskResponse.Failure;
                runwayJob.FailureCode = taskResponse.FailureCode;
                
                await dbContext.SaveChangesAsync(cancellationToken);
                _activeTasks.TryRemove(pollingTask.TaskId, out _);
                _logger.LogError($"Runway task {pollingTask.TaskId} failed: {taskResponse.Failure}");
            }
            else if (taskResponse.Status == "THROTTLED" || taskResponse.Status == "PENDING" || taskResponse.Status == "RUNNING")
            {
                // Task still in progress
                await dbContext.SaveChangesAsync(cancellationToken);
                
                pollingTask.RetryCount++;
                if (pollingTask.RetryCount >= _maxRetries)
                {
                    runwayJob.Status = "FAILED";
                    runwayJob.FailureReason = "Task timed out after 10 minutes";
                    await dbContext.SaveChangesAsync(cancellationToken);
                    _activeTasks.TryRemove(pollingTask.TaskId, out _);
                    _logger.LogError($"Runway task {pollingTask.TaskId} timed out");
                }
                else
                {
                    // Exponential backoff with max delay
                    pollingTask.CurrentDelay = TimeSpan.FromSeconds(Math.Min(pollingTask.CurrentDelay.TotalSeconds * 1.5, _maxDelay.TotalSeconds));
                    pollingTask.NextPollTime = DateTime.UtcNow.Add(pollingTask.CurrentDelay);
                    _logger.LogDebug($"Runway task {pollingTask.TaskId} still {taskResponse.Status}, next poll in {pollingTask.CurrentDelay.TotalSeconds}s");
                }
            }
            else
            {
                // Unknown status
                _logger.LogWarning($"Runway task {pollingTask.TaskId} has unknown status: {taskResponse.Status}");
                pollingTask.RetryCount++;
                if (pollingTask.RetryCount >= _maxRetries)
                {
                    _activeTasks.TryRemove(pollingTask.TaskId, out _);
                }
                else
                {
                    pollingTask.NextPollTime = DateTime.UtcNow.Add(pollingTask.CurrentDelay);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error polling Runway task {pollingTask.TaskId}");
            pollingTask.RetryCount++;
            if (pollingTask.RetryCount >= _maxRetries)
            {
                _activeTasks.TryRemove(pollingTask.TaskId, out _);
            }
            else
            {
                pollingTask.NextPollTime = DateTime.UtcNow.Add(pollingTask.CurrentDelay);
            }
        }
    }

    private string GetNotificationTitle(string taskType)
    {
        return taskType switch
        {
            "text_to_image" => "Image Generated!",
            "image_to_video" => "Video Created!",
            "video_upscale" => "Video Upscaled!",
            _ => "Runway Generation Complete!"
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Runway polling service is stopping");
        _activeTasks.Clear();
        await base.StopAsync(cancellationToken);
    }

    private class PollingTask
    {
        public string TaskId { get; set; } = string.Empty;
        public DateTime NextPollTime { get; set; }
        public TimeSpan CurrentDelay { get; set; }
        public int RetryCount { get; set; }
    }
}