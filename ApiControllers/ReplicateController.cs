using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using PhotoAiBackend.Services;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/replicate/")]
public class ReplicateController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly string _apiKey;
    private readonly INotificationService _notificationService;
    private readonly IFileService _fileService;

    public ReplicateController(AppDbContext dbContext, INotificationService notificationService, IFileService fileService)
    {
        _apiKey = Environment.GetEnvironmentVariable("ReplicateApiKey");
        _dbContext = dbContext;
        _notificationService = notificationService;
        _fileService = fileService;
    }
    
    private async Task<byte[]> DownloadImageAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();
    
        var response = await httpClient.GetAsync(imageUrl);
    
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to fetch image. Status: {response.StatusCode}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }

    [HttpPost("on-prediction-complete")]
    public async Task<IActionResult> OnPredictionComplete([FromQuery] Guid userId)
    {
        try
        {
            var body = await new StreamReader(Request.Body).ReadToEndAsync();

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine(body);
            Console.WriteLine("--------------------------------------------");
            
            var result = JsonSerializer.Deserialize<EnhanceCallbackPayload>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("Failed to fetch deserialize callback payload.");
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine(body);
                return BadRequest("Failed to deserialize payload.");
            }
            
            var foundJob = await _dbContext.EnhanceJobs.FirstOrDefaultAsync(j => j.Id == result.Id);

            if (result.Status != "succeeded")
            {
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine($"Failed to change the status of the job {result.Status}.");
                Console.WriteLine("-----------------------------------------------------------------");
                
                foundJob.Status = Enum.TryParse(result.Status, out EnhanceStatus status) ? status : EnhanceStatus.Failed;
                
                return Ok("Status changed to " + result.Status);
            }
            
            var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (foundUser == null)
            {
                return NotFound("User not found.");
            }
            
            if (result?.Output == null)
            {
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("Failed to parse the output.");
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine(result);
                return BadRequest("Failed to parse response.");
            }

            if (foundJob == null)
            {
                return NotFound("Job not found.");
            }
        
            var data = await DownloadImageAsync(result.Output);
            var url = await _fileService.UploadFile(data);

            var image = new EnhanceImage
            {
                Id = Guid.NewGuid(),
                JobId = foundJob.Id,
                ImageUrl = url
            };

            await _dbContext.EnhanceImages.AddAsync(image);
            foundJob.Status = EnhanceStatus.Succeeded;

            foundUser.Credits -= 10;
            
            await _dbContext.SaveChangesAsync();
        
            if (foundUser.FcmTokenId != null)
            {
                var notificationData = new Dictionary<string, string>
                {
                    { "type", GenerationType.Filter.ToString() },
                    { "jobId", foundJob.Id }
                };
            
                IReadOnlyDictionary<string, string> readOnlyData = new ReadOnlyDictionary<string, string>(notificationData);
            
                var notification = new NotificationInfo
                {
                    Title = "Photo Enhanced!",
                    Text = "Your image has been enhanced with AI. Tap to see the improved version."
                };  
                
                await _notificationService.SendNotificatino(foundUser.FcmTokenId, notification, readOnlyData);
            }
        
            return Ok("The message was received.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine(ex.Message);
            Console.WriteLine("-----------------------------------------------------------------");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("create-ghibli-prediction")]
    public async Task<IActionResult> CreateGhibliPrediction([FromQuery] Guid userId, [FromQuery] string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest("Missing image URL.");
        }

        try
        {
            var replicateApiUrl = "https://api.replicate.com/v1/predictions";
            var webhookUrl = $"https://image-generation-backend-164860087792.us-central1.run.app/api/replicate/on-prediction-complete?userId={userId}";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                version = "58219e0f4ab1c129d47a6e667d39033c174b4c8364d08b1fa30a94be501087bf",
                webhook = webhookUrl,
                input = new
                {
                    seed = 42,
                    width = 1248,
                    height = 1248,
                    lora_scale = 1,
                    control_type = "Ghibli",
                    spatial_img = imageUrl,
                    prompt = "Studio Ghibli style hand drawn image"
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(replicateApiUrl, content);

            if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.Created)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"❌ Request failed: {error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<EnhanceCallbackPayload>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (result == null)
            {
                return BadRequest("❌ Failed to parse response body.");
            }
            
            var enhanceJob = new EnhanceJob
            {
                Id = result.Id,
                Status = EnhanceStatus.Running,
                CreatedAt = result.CreatedAt,
                UserId = userId,
                EnhanceImages = []
            };
            
            await _dbContext.EnhanceJobs.AddAsync(enhanceJob);
            await _dbContext.SaveChangesAsync();

            return Ok(enhanceJob);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, $"❌ Error: {ex.Message}");
        }
    }
    
    [HttpPost("create-prediction")]
    public async Task<IActionResult> CreatePrediction([FromQuery] string imageUrl, [FromQuery] Guid userId)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest("Missing image URL.");
        }

        try
        {
            var replicateApiUrl = "https://api.replicate.com/v1/predictions";
            var webhookUrl = $"https://image-generation-backend-164860087792.us-central1.run.app/api/replicate/on-prediction-complete?userId={userId}";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                version = "f121d640bd286e1fdc67f9799164c1d5be36ff74576ee11c803ae5b665dd46aa",
                webhook = webhookUrl,
                input = new
                {
                    image = imageUrl,
                    face_enhance = true
                }
            };
            
            var json = JsonSerializer.Serialize(payload);
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(replicateApiUrl, content);
            
            if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.Created)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"❌ Request failed: {error}");
            }
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            var result = JsonSerializer.Deserialize<EnhanceCallbackPayload>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                return BadRequest("❌ Failed to parse response body.");
            }
            
            var enhanceJob = new EnhanceJob
            {
                Id = result.Id,
                Status = EnhanceStatus.Running,
                CreatedAt = result.CreatedAt,
                UserId = userId,
                EnhanceImages = []
            };
            
            await _dbContext.EnhanceJobs.AddAsync(enhanceJob);
            await _dbContext.SaveChangesAsync();

            return Ok(enhanceJob);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, $"❌ Error: {ex.Message}");
        }
    }
}