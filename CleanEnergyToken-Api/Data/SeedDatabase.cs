using Microsoft.AspNetCore.Components.Web;

namespace CleanEnergyToken_Api.Data
{
    public static class SeedDatabase
    {
        public static async Task EnsureSeedData(this IServiceProvider services)
        {
            await services.GetRequiredService<SeedRoles>().Seed();
            await services.GetRequiredService<SeedUsers>().Seed();
        }
    }
}
