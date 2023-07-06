using CleanEnergyToken_Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CleanEnergyToken_Api.Services
{
    public interface IIncentiveService
    {
        IncentiveModel? GetIncentiveRate();
        IncentiveModel GetIncentiveRate(decimal consumption);
    }
    public class IncentiveService : IIncentiveService
    {
        private const string CurrentIncentive = "CurrentIncentive";
        IMemoryCache _cache;
        IPowerConsumptionService _consumptionService;
        IPowerProductionService _productionService;


        public IncentiveService(IPowerConsumptionService consumptionService, IPowerProductionService productionService, IMemoryCache cache)
        {
            _consumptionService = consumptionService;
            _productionService = productionService;
            _cache = cache;
        }
        /// <summary>
        /// Calculate Incentives based on consumption and production rates
        /// - Compare Consumption Rate with Optimal Consumption Rate
        /// 1 - if Consumption Rate more than optimal :
        ///         NONE / Zero
        /// 2 - if Consumption Rate less than MaxIncentiveForConsumptionRate :
        ///         MaxIncentive
        /// 3 - On value in between :
        ///         Give Incentives as % of MaxIncentiveRate
        /// 
        /// Example / Illustration:
        /// 
        ///                                                                                                     Optimal
        ///                                                                                                     Consumption   
        ///                                                                                                     |
        /// |---------------------------------------------------------------------------------------------------|----------|
        /// |||||||||||||||||| MaxIncentive Area ||||||||||||||||||||~~~ Gradually Increasing Incentive Area ~~~|0Incentive|
        /// |-------------------------------------------------------|---------------------------------|---------|----------|
        ///                                                         50%                               74%       90%        100%
        ///                                                         |                                 |                    |
        ///                                                         Max Incentive                     Current              Max
        ///                                                         For Consumption Rate              Consumption          Production
        ///                                                                                           (~3.1% Incentive   
        ///                                                                                           Rate)              
        /// </summary>                                                                                                    
        /// <returns></returns>
        public IncentiveModel GetIncentiveRate(decimal consumption)
        {
            var max = _productionService.GetMaxProduction();

            return new IncentiveModel(consumption, max);
        }

        public IncentiveModel? GetIncentiveRate()
        {
            var inCache = _cache.TryGetValue(CurrentIncentive, out IncentiveModel? currentIncentiveRate);
            if (!inCache)
            {
                currentIncentiveRate = GetIncentiveRate(_consumptionService.GetCurrentConsumptionInWatt());
                _cache.Set(CurrentIncentive, currentIncentiveRate, TimeSpan.FromMinutes(5));
            }
            return currentIncentiveRate;
        }
    }
}
