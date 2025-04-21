namespace Tournament.Services.SignInManager
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class CustomSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        public CustomSignInManager(
            UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<TUser> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) { }

        public override async Task<ClaimsPrincipal> CreateUserPrincipalAsync(TUser user)
        {
            var principal = await base.CreateUserPrincipalAsync(user);

            if (user is Tournament.Data.Models.User appUser)
            {
                var identity = (ClaimsIdentity)principal.Identity;
                var roles = await UserManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return principal;
        }
    }
}
