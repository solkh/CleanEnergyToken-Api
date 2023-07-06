
using CleanEnergyToken_Api.Extentions;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace CleanEnergyToken_Api.ApiControllers
{
    [ApiController, Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<IncentivesController> _logger;
        private readonly IIncentiveService _incentiveService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<IncentivesController> logger, IIncentiveService incentiveService, IBlockChainService blockChainService, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _incentiveService = incentiveService;
            _blockChainService = blockChainService;
            _userManager = userManager;
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        public async Task<ActionResult<HomeDto>> GetHome()
        {
            var user = await _userManager.GetUserAsync(User);
            var userWalletAddress = user?.Address;
            var isPSW = false;
            if (userWalletAddress != null)
            {
                isPSW = await _userManager.IsInRoleAsync(user, "PowerStationWorker");
            }
            var incentives = _incentiveService.GetIncentiveRate();
            var res = new HomeDto
            {
                Balance = userWalletAddress != null ? (await _blockChainService.GetWalletBalanceAsync(userWalletAddress)).ToSMRGDecimal() : null,
                Incentive = incentives?.IncentiveRate ?? 0,
                MaxIncentive = IncentiveModel.MaxIncentiveRate,
                IsPowerStationWorker = isPSW
                WalletAddress = userWalletAddress
            };

            return res;
        }
    }
}