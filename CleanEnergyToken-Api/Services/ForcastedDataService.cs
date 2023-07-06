using CleanEnergyToken_Api.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CleanEnergyToken_Api.Services
{
    public interface IForcastedDataService
    {
        public decimal GetForcastedConsumption();
        public decimal GetForcastedConsumption(DateTime date);
    }

    public class ForcastedDataService : IForcastedDataService
    {
        private readonly List<PowerRecord> Data = new();
        private readonly ILogger<ForcastedDataService> _logger;
        public ForcastedDataService(ILogger<ForcastedDataService> logger)
        {
            _logger = logger;
            if (File.Exists("ForcastedData.csv"))
            {
                try
                {
                    using var reader = File.OpenText("ForcastedData.csv");
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = false });

                    csv.Context.RegisterClassMap<PowerRecordMap>();

                    Data.AddRange(csv.GetRecords<PowerRecord>().ToArray());
                }
                catch (Exception e)
                {
                    _logger?.LogError(e.Message);
                }
            }
        }

        public decimal GetForcastedConsumption() => GetForcastedConsumption(DateTime.Now);
        public decimal GetForcastedConsumption(DateTime date) =>
            Data.OrderBy(x => (x.Date - date)?.TotalMinutes??9999).FirstOrDefault()?.Wattage ?? 0m;
    }
}
