using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public interface IRunwayService
{
    Task<RunwayTaskResponse> CreateTextToImageTaskAsync(RunwayTextToImageRequest request);
    Task<RunwayTaskResponse> CreateImageToVideoTaskAsync(RunwayImageToVideoRequest request);
    Task<RunwayTaskResponse> CreateVideoUpscaleTaskAsync(RunwayVideoUpscaleRequest request);
    Task<RunwayTaskResponse> GetTaskStatusAsync(string taskId);
    Task<bool> CancelOrDeleteTaskAsync(string taskId);
    Task<RunwayOrganizationResponse> GetOrganizationInfoAsync();
}