
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
    public class TokenController : ControllerBase
    {
        private readonly ILogger<IncentivesController> _logger;
        private readonly IIncentiveService _incentiveService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;

        public TokenController(ILogger<IncentivesController> logger, IIncentiveService incentiveService, IBlockChainService blockChainService, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _incentiveService = incentiveService;
            _blockChainService = blockChainService;
            _userManager = userManager;
        }


        [HttpGet("{id}.json")]
        public ActionResult<TokenDto> GetHome(string id) =>
            new TokenDto
            {
                Name = "Smarge Token (Watt)",
                Description = "Optimize your power consumption !",
                Image = "https://cet.kuarkz.com/logo.png",
                Strength = 1
            };
    }
}