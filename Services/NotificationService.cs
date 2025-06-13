using FirebaseAdmin.Messaging;
using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public class NotificationService : INotificationService
{
    public async Task SendNotificatino(string fcmTokenId, NotificationInfo info, IReadOnlyDictionary<string, string> data)
    {
        Message message = new Message()
        {
            Token = fcmTokenId,
            Data = data,
            Notification = new Notification()
            {
                Title = info.Title,
                Body = info.Text,
            },
            Apns = new ApnsConfig()
            {
                Aps = new Aps()
                {
                    Sound = "sound.caf"
                }
            }
        };
        
        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

        Console.WriteLine(response);
    }
}