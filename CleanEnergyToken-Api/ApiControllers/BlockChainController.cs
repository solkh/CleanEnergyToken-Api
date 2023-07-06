using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using OpenIddict.Server.AspNetCore;
using System.Numerics;

namespace CleanEnergyToken_Api.ApiControllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlockChainController : ControllerBase
    {
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;
        public BlockChainController(IBlockChainService blockChainService, UserManager<AppUser> userManager)
        {
            _blockChainService = blockChainService;
            _userManager = userManager;
        }

        [HttpGet]
        public ActionResult<string> GetNewKey()
        {
            return string.Join(" ", _blockChainService.GenerateMnemonic());
        }

        [HttpGet("Balance/{address}")]
        public async Task<ActionResult<BigInteger>> Balance(string address) =>
            (await _blockChainService.GetWalletBalanceAsync(address));


        [HttpGet("Transactions/{address}")]
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<TransferEventDTO[]>> Transactions(string address) =>
            await _blockChainService.GetWalletTransactionsAsync(address);


        [HttpGet("Transactions")]
        [Authorize]
        public async Task<ActionResult<TransferEventDTO[]>> Transactions()
        {
            var address = (await _userManager.GetUserAsync(User))?.Address;
            if (address == null)
                return Array.Empty<TransferEventDTO>();

            var txs = await _blockChainService.GetWalletTransactionsAsync(address);

            return txs;
        }

        [HttpGet("Address")]
        [Authorize]
        public async Task<ActionResult<string>> Address() =>
            (await _userManager.GetUserAsync(User))?.Address ?? "";

        [HttpGet("GetPrivateKey/{mnemonic}")]
        [Authorize]
        public ActionResult<string> GetPrivateKey(string mnemonic)
        {
            var res = _blockChainService.GetWalletFromMnemonic(mnemonic);

            if (res == null)
                return base.BadRequest(ApiErr.Create("Invalid Mnemonic"));

            return res.PrivateKey;
        }

        [HttpGet("GenerateMnemonic")]
        public ActionResult<string[]> GenerateMnemonic() => _blockChainService.GenerateMnemonic();
    }
}
