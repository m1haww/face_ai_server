using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public interface IRunwayService
{
    Task<RunwayTaskResponse> CreateImageGenerationTaskAsync(RunwayImageGenerationRequest request);
    Task<RunwayTaskResponse> GetTaskStatusAsync(string taskId);
}