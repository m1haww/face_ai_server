using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;

namespace QwenChatBackend.ApiControllers;

[ApiController]
[Route("/api/file/")]
public class FileController : ControllerBase
{
    private readonly string _bucketName = "ai-assistant-macos-app";
    private readonly GoogleCredential _credential;

    public FileController()
    {
        var authJson = Environment.GetEnvironmentVariable("GCPStorageAuthFile") ?? "";
        _credential = GoogleCredential.FromJson(authJson);
    }
    
    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty or missing");

        var client = StorageClient.Create(_credential);

        using var stream = file.OpenReadStream();

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? GetContentType(file.FileName)
            : file.ContentType;

        var obj = await client.UploadObjectAsync(_bucketName, fileName, contentType, stream);

        var url = $"https://storage.googleapis.com/ai-assistant-macos-app/{obj.Name}";

        return Ok(new
        {
            message = "File uploaded successfully!",
            fileUrl = url
        });
    }
    
    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
    
    [HttpGet("get-file")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        var client = StorageClient.Create(_credential);

        var stream = new MemoryStream();
        await client.DownloadObjectAsync(_bucketName, fileName, stream);
        stream.Position = 0;

        var contentType = GetContentType(fileName);
        Response.Headers["Content-Disposition"] = "inline";

        return File(stream, contentType);
    }
}