namespace PhotoAiBackend.Services;

public interface IFileService
{
    Task<string> UploadFile(byte[] fileData);
}