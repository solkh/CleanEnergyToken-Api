using CleanEnergyToken_Api.Data;
using CleanEnergyToken_Api.Extentions;
using CleanEnergyToken_Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanEnergyToken_Api.Services
{
    public class NotifierHostedService : BackgroundService
    {
        private const int CheckUpdateTime = 60 * 60 * 1000; // 1 Hour in ms
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<NotifierHostedService> _logger;

        public NotifierHostedService(ILogger<NotifierHostedService> _logger,IServiceScopeFactory scopeFactory)
        {
            this._logger = _logger;
            this.scopeFactory = scopeFactory;
        }
        private async Task Execute(CancellationToken stoppingToken)
        {
            var scope = scopeFactory.CreateScope();
            if (scope == null || scope.ServiceProvider == null)
                return;

            var _notificationService = scope.ServiceProvider.GetService<INotificationService>();
            var _forcastedDataService = scope.ServiceProvider.GetService<IForcastedDataService>();
            var _incentiveService = scope.ServiceProvider.GetService<IIncentiveService>();
            var _userManager = scope.ServiceProvider.GetService<UserManager<AppUser>>();

            if (_notificationService == null || _forcastedDataService == null || _incentiveService == null || _userManager == null)
                return;

            var consumptionAfterAnHour = _forcastedDataService.GetForcastedConsumption(DateTime.UtcNow.AddHours(1));
            var forcastedIncentives = _incentiveService.GetIncentiveRate(consumptionAfterAnHour);
            var forcastedIncentiveRate = forcastedIncentives.IncentiveRate;
            var all = await _userManager.Users.Select(x => x.Id).ToArrayAsync();
            await _notificationService.SendNotableForcastedIncentive(forcastedIncentiveRate, all);
        }

        #region Internal ExecuteAsync
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"{nameof(NotifierHostedService)} is starting.");

            stoppingToken.Register(() =>
                _logger.LogDebug($" {nameof(NotifierHostedService)} background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Task doing background work.");


                try
                {
                    await Execute(stoppingToken);

                    await Task.Delay(CheckUpdateTime, stoppingToken);
                }
                catch (TaskCanceledException exception)
                {
                    _logger.LogCritical(exception, "TaskCanceledException Error", exception.Message);
                }
            }

            _logger.LogDebug($"Background task is stopping.");
        }
        #endregion
    }
}
