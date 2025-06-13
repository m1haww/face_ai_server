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
        // _apiKey = Environment.GetEnvironmentVariable("RunAwayKey") ?? throw new InvalidOperationException("RunAwayKey not configured");
        _apiKey =
            "key_b983e22f13ca4363b28b42c96e76d54ae7c9ab11d3036e539820c34cbfa34a3240573db0131e53033419c8b14c4077b891eba73b346e2b72e4f8b0b22e78a02c";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("X-Runway-Version", "2024-11-06");
    }

    public async Task<RunwayTaskResponse> CreateTextToImageTaskAsync(RunwayTextToImageRequest request)
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

    public async Task<RunwayTaskResponse> CreateImageToVideoTaskAsync(RunwayImageToVideoRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/image_to_video", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Runway API error: {response.StatusCode} - {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RunwayTaskResponse>(responseBody) ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task<RunwayTaskResponse> CreateVideoUpscaleTaskAsync(RunwayVideoUpscaleRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/video_upscale", content);
        
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

    public async Task<bool> CancelOrDeleteTaskAsync(string taskId)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/tasks/{taskId}");
        
        return response.StatusCode == System.Net.HttpStatusCode.NoContent || response.StatusCode == System.Net.HttpStatusCode.NotFound;
    }

    public async Task<RunwayOrganizationResponse> GetOrganizationInfoAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/organization");
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Runway API error: {response.StatusCode} - {error}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RunwayOrganizationResponse>(responseBody) ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}