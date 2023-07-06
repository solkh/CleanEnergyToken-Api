using Microsoft.Extensions.Caching.Memory;

namespace CleanEnergyToken_Api.Services
{
    public interface IPowerProductionService
    {
        /// <summary>
        /// This should get power production from realtime readings / sensors
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrentProductionInWatt();

        public decimal GetMaxProduction();
    }
    public class PowerProductionService : IPowerProductionService
    {
        private const string CurrentProductionInWatt = "CurrentProductionInWatt";
        IForcastedDataService _service;
        IMemoryCache _memoryCache;
        Random rnd;
        public PowerProductionService(IForcastedDataService service, IMemoryCache memoryCache)
        {
            _service = service;
            _memoryCache = memoryCache;
            rnd = new Random();
        }

        public decimal GetCurrentProductionInWatt()
        {
            var inCache = _memoryCache.TryGetValue(CurrentProductionInWatt, out decimal currentProduction);
            if (!inCache)
            {
                var forcastedConsumption = _service.GetForcastedConsumption();
                // Calc Random Error Rate (+- 5%) off Forcasted Consumption Rate
                var errorRate = (decimal)(rnd.NextDouble() - 0.5) / 10;
                currentProduction = forcastedConsumption + (forcastedConsumption * errorRate);
                _memoryCache.Set(CurrentProductionInWatt, currentProduction, TimeSpan.FromMinutes(5));
            }
            return currentProduction;
        }

        // 1 MW
        public decimal GetMaxProduction() => 1_000_000m;
    }
}
