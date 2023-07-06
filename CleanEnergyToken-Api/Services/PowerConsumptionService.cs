using Microsoft.Extensions.Caching.Memory;

namespace CleanEnergyToken_Api.Services
{
    public interface IPowerConsumptionService
    {
        /// <summary>
        /// This should get power consumption from realtime readings / sensors
        /// </summary>
        /// <returns></returns>
        public decimal GetCurrentConsumptionInWatt();
    }
    public class PowerConsumptionService : IPowerConsumptionService
    {
        private const string CurrentConsumptionInWatt = "CurrentConsumptionInWatt";
        IPowerProductionService _service;
        IMemoryCache _memoryCache;
        Random rnd;
        public PowerConsumptionService(IPowerProductionService service, IMemoryCache memoryCache)
        {
            _service = service;
            _memoryCache = memoryCache;
            rnd = new Random();
        }

        public decimal GetCurrentConsumptionInWatt()
        {
            var inCache = _memoryCache.TryGetValue(CurrentConsumptionInWatt, out decimal currentConsumption);
            if (!inCache)
            {
                var currentProduction = _service.GetCurrentProductionInWatt();
                // Calc Random Variance Rate (- 10%) off Current Production Rate
                var errorRate = (decimal)(rnd.NextDouble() - 1) / 10;
                currentConsumption = currentProduction + (currentProduction * errorRate);
                _memoryCache.Set(CurrentConsumptionInWatt, currentConsumption, TimeSpan.FromMinutes(5));
            }
            return currentConsumption;
        }
    }
}
