using System.Numerics;

namespace CleanEnergyToken_Api.Extentions
{
    public static class BigIntegerExt
    {
        private const decimal dicimalPlaces = 1000_000_000_000_000_000m;
        public static decimal ToSMRGDecimal(this BigInteger bi) => ((decimal)bi) / dicimalPlaces;
        public static BigInteger ToSMRGBigInteger(this decimal d) => new(d * dicimalPlaces);
    }
}
