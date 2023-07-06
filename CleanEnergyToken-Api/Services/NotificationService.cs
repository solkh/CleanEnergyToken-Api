using CleanEnergyToken_Api.Data;
using CleanEnergyToken_Api.Entities;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace CleanEnergyToken_Api.Services
{
    public interface INotificationService
    {
        Task SendPushNotification(Notification n, string[] recivers);

    }
    public class NotificationService : INotificationService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger _logger;
        private readonly AppDbContext _db;
        public NotificationService(ILogger<NotificationService> logger, IWebHostEnvironment env, AppDbContext db)
        {
            _env = env;
            _logger = logger;
            _db = db;
        }

        public async Task SendPushNotification(Notification n, string[] recivers)
        {
            try
            {
                string path = _env.ContentRootPath + "\\adminsdk.json";

                if (FirebaseApp.DefaultInstance == null)
                    FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromFile(path) });

                _db.Notifications.Add(n);
                await _db.SaveChangesAsync();

                var nms = recivers.Select(x => new NotificationMessage { NotificationId = n.Id, UserId = x }).ToArray();

                _db.NotificationMessages.AddRange(nms);
                await _db.SaveChangesAsync();

                if (FirebaseApp.DefaultInstance != null)
                {
                    var notif = new FirebaseAdmin.Messaging.Notification { Title = n.Title, Body = n.Text, ImageUrl = n.Image };
                    // Image config for iOS APNS
                    var apns = new FirebaseAdmin.Messaging.ApnsConfig { Aps = new FirebaseAdmin.Messaging.Aps { MutableContent = true }, FcmOptions = new FirebaseAdmin.Messaging.ApnsFcmOptions { ImageUrl = n.Image } };

                    var msg = new FirebaseAdmin.Messaging.Message { Notification = notif, Data = n.Payload, Topic = "all", Apns = apns };
                    var result = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(msg);
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e.ToString());
            }
        }
    }
}
