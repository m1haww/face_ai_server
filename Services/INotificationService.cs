using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public interface INotificationService
{
    Task SendNotificatino(string fcmTokenId, NotificationInfo info, IReadOnlyDictionary<string, string> data);
}