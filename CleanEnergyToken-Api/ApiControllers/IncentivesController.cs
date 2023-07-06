using CleanEnergyToken_Api.Extentions;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using OpenIddict.Validation.AspNetCore;
using System.Numerics;

namespace CleanEnergyToken_Api.ApiControllers
{
    [ApiController, Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Route("[controller]")]
    public class IncentivesController : ControllerBase
    {
        private readonly ILogger<IncentivesController> _logger;
        private readonly IIncentiveService _incentiveService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;

        public IncentivesController(ILogger<IncentivesController> logger, IIncentiveService incentiveService, IBlockChainService blockChainService, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _incentiveService = incentiveService;
            _blockChainService = blockChainService;
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult<IncentiveModel?> GetIncentiveRate() =>
            _incentiveService.GetIncentiveRate();


        [HttpPost("Send/{toaddress}/{totalWattCharged}")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = "PowerStationWorker")]
        public async Task<ActionResult<TransactionReceipt?>> SendIncentive(string toaddress, decimal totalWattCharged)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null)
                return base.BadRequest(ApiErr.Create("Invalid sender"));

            var reciver = await _userManager.Users.FirstOrDefaultAsync(x => x.Address == toaddress);
            if (reciver == null)
                return base.BadRequest(ApiErr.Create("Invalid reciver " + toaddress));

            var incentives = _incentiveService.GetIncentiveRate();
            var rate = incentives?.IncentiveRate ?? 0;
            var amount = (totalWattCharged * rate).ToSMRGBigInteger();

            if (amount <= 0)
                return base.BadRequest(ApiErr.Create("No incentives for this charge!"));

            var balance = await _blockChainService.GetWalletBalanceAsync(sender.Address);

            if (balance < amount)
                return base.BadRequest(ApiErr.Create("Insufficiant Funds (" + balance + ") in address " + sender.Address +". required amount: " + amount));

            var pk = CETEncryption.Dec(sender.PK, sender.Address);
            var receipt = await _blockChainService.SafeTransferCET(pk, sender.Address, reciver.Address, amount);

            return receipt;
        }
    }
}