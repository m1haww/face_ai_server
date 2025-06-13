using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using PhotoAiBackend.Models;

namespace PhotoAiBackend.Services;

public class NotificationService : INotificationService
{
    public async Task SendNotificatino(string fcmTokenId, NotificationInfo info, IReadOnlyDictionary<string, string> data)
    {
        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                Console.WriteLine("Firebase is not initialized. Skipping notification send.");
                return;
            }

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

            Console.WriteLine($"Successfully sent notification: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification: {ex.Message}");
        }
    }
}