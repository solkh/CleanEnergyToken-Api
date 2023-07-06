using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanEnergyToken_Api.Data
{
    public class SeedRoles
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task Seed()
        {
            await CreateRole("Admin");
            await CreateRole("PowerStationWorker");
            await CreateRole("User");
        }

        private async Task CreateRole(string role)
        {
            var dbrole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.NormalizedName == role.ToUpper());
            if (dbrole == null)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole() { Name = role });
            }
        }
    }
}
