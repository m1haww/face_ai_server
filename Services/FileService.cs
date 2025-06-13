using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace PhotoAiBackend.Services;

public class FileService : IFileService
{
    private readonly string _bucketName = "ai-assistant-macos-app";
    private readonly GoogleCredential _credential;

    public FileService()
    {
        var authJson = Environment.GetEnvironmentVariable("GCPStorageAuthFile") ?? "";
        _credential = GoogleCredential.FromJson(authJson);
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
    
    public async Task<string> UploadFile(byte[] fileData)
    {
        var baseUrl = "https://storage.googleapis.com/ai-assistant-macos-app/";
        var client = StorageClient.Create(_credential);
        
        var fileName = $"{Guid.NewGuid()}.png";
        var contentType = "image/png";

        using var stream = new MemoryStream(fileData);

        var obj = await client.UploadObjectAsync(
            bucket: _bucketName,
            objectName: fileName,
            contentType: contentType,
            source: stream
        );

        return $"{baseUrl}{obj.Name}";
    }
}