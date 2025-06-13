using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoAiBackend.Models;
using PhotoAiBackend.Persistance;
using PhotoAiBackend.Persistance.Entities;
using PhotoAiBackend.Services;

namespace PhotoAiBackend.ApiControllers;

[ApiController]
[Route("/api/stability/")]
public class StabilityAiController : ControllerBase
{
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.stability.ai/v2beta/stable-image/upscale/fast";
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IFileService _fileService;

    public StabilityAiController(AppDbContext dbContext, INotificationService notificationService, IFileService fileService)
    {
        _apiKey = Environment.GetEnvironmentVariable("StabilityAiApiKey");
        _dbContext = dbContext;
        _notificationService = notificationService;
        _fileService = fileService;
    }

    private byte[] GetFileArray(IFormFile file)
    {
        using (var ms = new MemoryStream())
        {
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
    
    [HttpPost("remove-background")]
    public async Task<IActionResult> RemoveBackground([FromForm] IFormFile image, [FromQuery] Guid userId, string outputFormat = "png")
    {
        try
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            var stabilityApiUrl = "https://api.stability.ai/v2beta/stable-image/edit/remove-background";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/*"));

                var content = new MultipartFormDataContent();

                var imageContent = new ByteArrayContent(GetFileArray(image));
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(imageContent, "\"image\"", "\"filename.jpg\"");

                if (!string.IsNullOrWhiteSpace(outputFormat))
                {
                    content.Add(new StringContent(outputFormat), "\"output_format\"");
                }

                var response = await httpClient.PostAsync(stabilityApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("------------------------------------------");
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(error);
                    Console.WriteLine("------------------------------------------");
                    return StatusCode((int)response.StatusCode, error);
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                var jobId = Guid.NewGuid().ToString();

                var job = new EnhanceJob
                {
                    Id = jobId,
                    UserId = userId,
                    Status = EnhanceStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow
                };

                var url = await _fileService.UploadFile(imageBytes);

                var enhanceImage = new EnhanceImage
                {
                    Id = Guid.NewGuid(),
                    JobId = jobId,
                    ImageUrl = url
                };

                var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (foundUser == null)
                {
                    return NotFound("User not found.");
                }

                foundUser.Credits -= 5;

                await _dbContext.EnhanceJobs.AddAsync(job);
                await _dbContext.EnhanceImages.AddAsync(enhanceImage);
                await _dbContext.SaveChangesAsync();

                if (foundUser.FcmTokenId != null)
                {
                    var notificationData = new Dictionary<string, string>
                    {
                        { "type", GenerationType.Filter.ToString() },
                        { "jobId", jobId }
                    };
                
                    IReadOnlyDictionary<string, string> readOnlyData = new ReadOnlyDictionary<string, string>(notificationData);
                
                    var notification = new NotificationInfo
                    {
                        Title = "Background Removed!",
                        Text = "Your image background has been successfully removed. Tap to view the result."
                    };
                
                    await _notificationService.SendNotificatino(foundUser.FcmTokenId, notification, readOnlyData);
                }

                return Ok(job);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine(ex.Message);
            Console.WriteLine("---------------------------------");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}