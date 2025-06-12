using Jahacki_klub_Zeljeznicar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


namespace Jahacki_klub_Zeljeznicar.Attributes
{

    public class KategorijaRequirement : IAuthorizationRequirement
    {
        public Kategorija[] AllowedKategorije { get; }

        public KategorijaRequirement(params Kategorija[] allowedKategorije)
        {
            AllowedKategorije = allowedKategorije;
        }
    }




    public class KategorijaHandler : AuthorizationHandler<KategorijaRequirement>
    {
        private readonly UserManager<User> _userManager;

        public KategorijaHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, KategorijaRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && requirement.AllowedKategorije.Contains(user.Kategorija))
            {
                context.Succeed(requirement);
            }
        }

    }

}
