using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public class RunwayService : IRunwayService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.dev.runwayml.com/v1";

    public RunwayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("RunwayApiKey") ?? throw new InvalidOperationException("RunwayApiKey not configured");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("X-Runway-Version", "2024-11-06");
    }

    public async Task<RunwayTaskResponse> CreateImageGenerationTaskAsync(RunwayImageGenerationRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/text_to_image", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Runway API error: {response.StatusCode} - {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RunwayTaskResponse>(responseBody) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<RunwayTaskResponse> GetTaskStatusAsync(string taskId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/tasks/{taskId}");
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Runway API error: {response.StatusCode} - {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RunwayTaskResponse>(responseBody) ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}