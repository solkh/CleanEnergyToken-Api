using System.Numerics;

namespace CleanEnergyToken_Api.Models
{
    public class HomeDto
    {
        public decimal MaxIncentive { get; set; }
        public decimal Incentive { get; set; }
        public decimal? Balance { get; set; }
        public bool? IsPowerStationWorker { get; set; } = false;
        public string? WalletAddress { get; set; }
    }
}
