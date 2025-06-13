using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using PhotoAiBackend.Services;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/image/")]
public class ImageGenController : ControllerBase
{
    private readonly string _apiKey;
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public ImageGenController(AppDbContext dbContext, INotificationService notificationService)
    {
        _apiKey = Environment.GetEnvironmentVariable("AstriaAiApiKey");
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    [HttpPost("generate-headshot")]
    public async Task<IActionResult> GenerateHeadshot([FromQuery] int? tempJobId, [FromBody] ImageGenerationPayload? payload = null)
    {
        try
        {
            if (tempJobId == null && payload == null)
                return BadRequest("Payload is required when tempJobId is not provided.");

            ImageJob job;
            string prompt;
            Guid userId;
            PresetCategory category;
            int tuneId;

            if (tempJobId == null)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == payload.UserId);
                if (user == null) return BadRequest("User not found");

                tuneId = user.TuneId;

                prompt = payload.Prompt;
                userId = payload.UserId;
                category = Enum.TryParse<PresetCategory>(payload.PresetCategory, true, out var parsedCategory) ? parsedCategory : PresetCategory.Headshots;

                job = new ImageJob
                {
                    Id = 0,
                    CreationDate = DateTime.UtcNow,
                    Status = JobStatus.Processing,
                    SystemPrompt = prompt,
                    UserId = userId,
                    Images = "[]",
                    PresetCategory = category
                };
            }
            else
            {
                var tempJob = await _dbContext.ImageJobs.FirstOrDefaultAsync(j => j.Id == tempJobId);
                if (tempJob == null) return BadRequest("Temp job not found");

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == tempJob.UserId);
                if (user == null) return BadRequest("User not found");
                
                tuneId = user.TuneId;

                prompt = tempJob.SystemPrompt;
                userId = tempJob.UserId;
                category = tempJob.PresetCategory;

                _dbContext.ImageJobs.Remove(tempJob);

                job = new ImageJob
                {
                    Id = 0,
                    CreationDate = DateTime.UtcNow,
                    Status = JobStatus.Processing,
                    SystemPrompt = prompt,
                    UserId = userId,
                    Images = "[]",
                    PresetCategory = category
                };
            }

            var jobInfo = await PostToAstriaAsync(prompt, tuneId, userId);

            job.Id = jobInfo.Id;
            job.CreationDate = jobInfo.CreatedAt;
            job.SystemPrompt = jobInfo.Text;

            await _dbContext.ImageJobs.AddAsync(job);
            await _dbContext.SaveChangesAsync();

            return Ok(job);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }

    private async Task<ImageGenerationResponse> PostToAstriaAsync(string prompt, int tuneId, Guid userId)
    {
        var values = new Dictionary<string, string>
        {
            { "prompt[text]", $"<lora:{tuneId}:1> {prompt}" },
            { "prompt[callback]", $"https://image-generation-backend-164860087792.us-central1.run.app/api/image/on-image-generated?userId={userId}" }
        };

        using var content = new FormUrlEncodedContent(values);
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await httpClient.PostAsync("https://api.astria.ai/tunes/1504944/prompts", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ImageGenerationResponse>(responseBody);
    }

    [HttpPost("on-image-generated")]
    public async Task<IActionResult> OnImageGenerated([FromQuery] Guid userId)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var requestBody = JsonSerializer.Deserialize<ImageGenerationCallbackPayload>(body);

        var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (requestBody == null)
        {
            return BadRequest("The request callback body is invalid.");
        }

        if (foundUser == null)
        {
            return BadRequest("The user couldn't be found.");
        }

        var foundJob = await _dbContext.ImageJobs.FirstOrDefaultAsync(ij => ij.Id == requestBody.Prompt.Id);

        if (foundJob == null)
        {
            return BadRequest("The job couldn't be found.");
        }
        
        foundJob.Images = JsonSerializer.Serialize(requestBody.Prompt.Images);
        foundJob.Status = JobStatus.Done;

        foundUser.Credits -= 15;

        await _dbContext.SaveChangesAsync();
        
        if (foundUser.FcmTokenId != null)
        {
            var data = new Dictionary<string, string>
            {
                { "type", GenerationType.Headshot.ToString() },
                { "jobId", foundJob.Id.ToString() }
            };
        
            IReadOnlyDictionary<string, string> readOnlyData = new ReadOnlyDictionary<string, string>(data);
        
            var notification = new NotificationInfo
            {
                Title = "Headshot is Ready!",
                Text = "Your AI-generated headshot is complete. Tap to view your results."
            };
            
            await _notificationService.SendNotificatino(foundUser.FcmTokenId, notification, readOnlyData);
        }
        
        return Ok("Success");
    }

    [HttpPost("tune-model")]
    public async Task<IActionResult> TuneModel([FromForm] List<IFormFile> images, [FromForm] string gender, [FromForm] Guid userId, [FromForm] string prompt, [FromForm] string presetCategory)
    {
        try
        {
            if (!images.Any())
            {
                return BadRequest("No images uploaded.");
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(Guid.NewGuid().ToString()), "tune[title]");
            content.Add(new StringContent(gender), "tune[name]");
            content.Add(new StringContent("flux-lora-portrait"), "tune[preset]");
            content.Add(new StringContent("1504944"), "tune[base_tune_id]");
            content.Add(new StringContent("lora"), "tune[model_type]");

            var tempJob = new ImageJob
            {
                Id = new Random().Next(1, 1000),
                UserId = userId,
                Status = JobStatus.Processing,
                SystemPrompt = prompt,
                CreationDate = DateTime.UtcNow,
                Images = "[]",
                PresetCategory = Enum.TryParse<PresetCategory>(presetCategory, true, out var parsedCategory)
                    ? parsedCategory
                    : PresetCategory.Headshots
            };
            
            var callbackUrl = $"https://image-generation-backend-164860087792.us-central1.run.app/api/image/generate-headshot?tempJobId={tempJob.Id}";
            
            content.Add(new StringContent(callbackUrl), "tune[callback]");

            foreach (var image in images)
            {
                var streamContent = new StreamContent(image.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(streamContent, "tune[images][]", image.FileName);
            }

            var response = await httpClient.PostAsync("https://api.astria.ai/tunes", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var result = await response.Content.ReadAsStringAsync();

            var decoded = JsonSerializer.Deserialize<TuneModelResponse>(result);

            if (decoded == null)
            {
                return BadRequest("The request callback body is invalid.");
            }
            
            var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (foundUser == null)
            {
                return BadRequest("The user couldn't be found.");
            }
            
            foundUser.TuneId = decoded.Id;

            await _dbContext.ImageJobs.AddAsync(tempJob);
            await _dbContext.SaveChangesAsync();
            
            return Ok("Tune has started.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return BadRequest(e.Message);
        }
    }
}
