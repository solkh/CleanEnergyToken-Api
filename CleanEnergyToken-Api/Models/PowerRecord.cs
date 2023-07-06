using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace CleanEnergyToken_Api.Models
{
    public class PowerRecord
    {
        [Index(0)]
        public string DateStr { get; set; }

        [Index(1)]
        public decimal Wattage { get; set; }
        public DateTime? Date => DateStr !=null? DateTime.Parse(DateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind):null;
    };

    public sealed class PowerRecordMap : ClassMap<PowerRecord>
    {
        public PowerRecordMap()
        {
            Map(f => f.Date).Index(0);
            Map(f => f.Wattage).Index(1);
        }
    }
}
