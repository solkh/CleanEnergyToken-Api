using CleanEnergyToken_Api.Entities;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using System.Numerics;

namespace CleanEnergyToken_Api.Extentions
{
    public static class NotificationServiceExtensions
    {
        public static async Task SendNotableForcastedIncentive(this INotificationService service, decimal forcastedIncentiveRate, string[] recivers)
        {
            if (forcastedIncentiveRate <= 0) return;
            var prority = forcastedIncentiveRate / IncentiveModel.MaxIncentiveRate;
            var percent = (int)forcastedIncentiveRate * 100;
            var n = new Notification
            {
                Title = $"Win {percent}% on every charge!",
                Text = "Get ready and charge your vehicle in an hour, and win SMRG for easy refills!"
            };
            await service.SendPushNotification(n, recivers);
        }
        public static async Task SendCETRecived(this INotificationService service, string userid, BigInteger amount)
        {
            var n = new Notification
            {
                Title = $"{amount} SMRG recived",
                Text = "Thanks for caring about clean enery, waiting you for your next charge"
            };
            await service.SendPushNotification(n, new[] { userid });
        }

    }
}
