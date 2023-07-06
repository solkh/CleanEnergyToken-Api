using CleanEnergyToken_Api.Services;

namespace CleanEnergyToken_Api.Models
{
    public class IncentiveModel
    {
        public IncentiveModel(decimal currentConsumption, decimal maxProduction)
        {
            CurrentConsumption = currentConsumption;
            MaxProduction = maxProduction;
        }
        public static readonly decimal OptimalConsumptionRate = 0.9m;
        public static readonly decimal MaxIncentiveRate = 0.1m; // per charged watts
        public static readonly decimal MaxIncentiveForConsumptionRate = 0.5m; // MaxIncentiveRate will be given in FULL if ConsumptionRate is below this rate

        public decimal CurrentConsumption { get; set; }
        public decimal MaxProduction { get; set; }
        public decimal ConsumptionRate =>
            CurrentConsumption / MaxProduction;
        public decimal IncentiveRate
        {
            get
            {
                if (ConsumptionRate >= OptimalConsumptionRate)
                    return 0;

                if (ConsumptionRate <= MaxIncentiveForConsumptionRate)
                    return MaxIncentiveRate;

                return ((OptimalConsumptionRate - ConsumptionRate) / MaxIncentiveForConsumptionRate) * MaxIncentiveRate;
            }

        }
    }
}

