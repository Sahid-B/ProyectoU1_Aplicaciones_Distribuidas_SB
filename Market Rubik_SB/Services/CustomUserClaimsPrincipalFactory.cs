using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MiniMarketApp.Services
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public CustomUserClaimsPrincipalFactory(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
            _userManager = userManager;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var email = user.Email ?? user.UserName ?? string.Empty;

            // Si el usuario no es el Admin principal "admin@marketrubik.com"
            if (!email.Equals("admin@marketrubik.com", StringComparison.OrdinalIgnoreCase))
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    // Remover cualquier claim de Admin accidental
                    var adminClaims = identity.FindAll(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
                    foreach (var claim in adminClaims.ToList())
                    {
                        identity.RemoveClaim(claim);
                    }

                    // Asegurar rol Cliente
                    if (!await _userManager.IsInRoleAsync(user, "Cliente"))
                    {
                        await _userManager.AddToRoleAsync(user, "Cliente");
                    }

                    if (!identity.HasClaim(ClaimTypes.Role, "Cliente"))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Cliente"));
                    }
                }
            }

            return identity;
        }
    }
}
