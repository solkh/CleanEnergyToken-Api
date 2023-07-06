using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Microsoft.AspNetCore;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using CleanEnergyToken_Api.Models;
using CleanEnergyToken_Api.Services;
using Microsoft.EntityFrameworkCore;
using CleanEnergyToken_Api.Extentions;

namespace App.ApiControllers.Authorization
{
    //[ApiVersion("1"), ApiVersion("2")]
    public class OAuthTokenController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _service;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger _logger;

        public OAuthTokenController(
            ILogger<OAuthTokenController> logger,
            IWebHostEnvironment env,
            IBlockChainService service,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager)
        {
            _env = env;
            _service = service;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            try
            {
                if (request.IsPasswordGrantType())
                {
                    var wallet = _service.GetWalletFromMnemonic(request.Password);
                    if (wallet == null)
                        return ForbidInvalidUsernamePassword();
                    var userName = CETEncryption.Hash(wallet.PublicKey);
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                    if (user == null)
                    {
                        var email = wallet.Address + "@cet.kuarkz.com";
                        user = new AppUser()
                        {
                            FullName = request?.Username ?? email,
                            Email = email,
                            EmailConfirmed = true,
                            UserName = userName,
                            Address = wallet.Address,
                            PK = CETEncryption.Enc(wallet.PrivateKey, wallet.Address),
                            NormalizedUserName = email,
                            CreatedDate = DateTime.UtcNow
                        };
                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            Console.WriteLine(string.Join("\r\n", result.Errors.ToList().Select(x => x.Code + " - " + x.Description)));
                        }
                        result = await _userManager.AddToRoleAsync(user, "User");
                        await _userManager.UpdateAsync(user);
                    }
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                    return await SignIn(user, request);
                }
                else if (request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType())
                {
                    var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

                    var user = await _userManager.GetUserAsync(principal);

                    if (user == null)
                        return ForbidInvalidToken();

                    if (!await _signInManager.CanSignInAsync(user))
                        return ForbidNolongerAllowed();

                    return await SignIn(user, request);
                }
            }
            catch (Exception e)
            {
                return ForbidException(e);
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        #region SignIn Helper Methods
        private async Task<Microsoft.AspNetCore.Mvc.SignInResult> SignIn(AppUser user, OpenIddictRequest request)
        {
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);

            claimsPrincipal.SetScopes(new[] { Scopes.OpenId, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess }.Intersect(request.GetScopes()));

            foreach (var claim in claimsPrincipal.Claims)
                claim.SetDestinations(ClaimDestination.Get(claim, claimsPrincipal));

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        #endregion

        #region Forbid Helper Methods
        private ForbidResult Forbid(string title, string error, string desc)
        {
            _logger.LogError(title + ":" + error + ":" + desc);
            return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                          properties: new AuthenticationProperties(new Dictionary<string, string?>
                          {
                              [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                              [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = desc
                          }));
        }
        private ForbidResult ForbidInvalidUsernamePassword() =>
            Forbid("ForbidInvalidUsernamePassword", Errors.AccessDenied, "InvalidUsernamePassword");

        private ForbidResult ForbidNotConfirmed() =>
            Forbid("ForbidNotConfirmed", Errors.AccessDenied, "UserAccountNotConfirmed");

        private ForbidResult ForbidInactive() =>
            Forbid("ForbidInactive", Errors.AccessDenied, "UserAccountInactive");

        private ForbidResult ForbidException(Exception e) =>
            Forbid("ForbidException: " + e, Errors.InvalidGrant, "AuthorizeException");

        private ForbidResult ForbidNolongerAllowed() =>
            Forbid("NolongerAllowed", Errors.InvalidGrant, "NolongerAllowed");

        private ForbidResult ForbidInvalidToken() =>
            Forbid("ForbidInvalidRefreshToken", Errors.InvalidGrant, "InvalidToken");
        #endregion
    }
}
