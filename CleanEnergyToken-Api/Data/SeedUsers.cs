using CleanEnergyToken_Api.Extentions;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanEnergyToken_Api.Data
{
    public class SeedUsers
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _service;
        public SeedUsers(UserManager<AppUser> userManager, IBlockChainService service)
        {
            _userManager = userManager;
            _service = service;
        }

        public async Task Seed()
        {
            await CreateUserInRole("Admin", "decide pipe skate picnic rack people discover avocado panther exclude that gas");
            //await CreateUserInRole("PowerStationWorker", "position occur height base legal seven journey license any assault prosper mention");
            await CreateUserInRole("PowerStationWorker", "tragic parade crisp rescue that media like want jealous pig silver amateur");
            await CreateUserInRole("User", "love casual winner unlock tackle talent symbol mountain time depart fix girl");
            await Task.CompletedTask;
        }

        public async Task CreateUserInRole(string role, string mnemonic)
        {
            var wallet = _service.GetWalletFromMnemonic(mnemonic);
            if (wallet == null)
                throw new Exception("Invalid Mnemonic");

            var username = CETEncryption.Hash(wallet.PublicKey);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user != null)
                return;

            var email = role + "@cet.kuarkz.com";
            var pkemail = wallet.Address + "@cet.kuarkz.com";
            user = new AppUser()
            {
                FullName = email,
                Email = email,
                EmailConfirmed = true,
                UserName = username,
                Address = wallet.Address,
                PK = CETEncryption.Enc(wallet.PrivateKey, wallet.Address),
                NormalizedUserName = email,
                CreatedDate = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(user, role);
            }
            await _userManager.UpdateAsync(user);
        }
    }
}
