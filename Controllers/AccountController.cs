using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IEvangelist.VideoChat.Controllers
{
    [
        Authorize,
        Route("api/account")
    ]
    public class AccountController : Controller
    {
        [HttpGet("profile")]
        public IActionResult Profile()
            => Json(new { name = User.Identity.Name, email = User.FindFirst(ClaimTypes.Email)?.Value });


        [HttpGet("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [
            AllowAnonymous, 
            HttpGet("providers")
        ]
        public async Task<IActionResult> GetProviders([FromServices] IAuthenticationSchemeProvider schemeProvider)
        {
            var providers = (await schemeProvider.GetAllSchemesAsync()).Where(s => !string.IsNullOrWhiteSpace(s.DisplayName));
            return Json(providers.Select(p => new { name = p.DisplayName }));
        }

        [
            AllowAnonymous,
            HttpGet("signIn/{provider}")
        ]
        public IActionResult SignIn([FromRoute] string provider)
        {
            var properties = CreateAuthenticationProperties(provider, "/");
            return new ChallengeResult(provider, properties);
        }

        //public IActionResult OnGetCallback(string provider = null, string remoteError = null)
        //{
        //    if (remoteError != null)
        //    {

        //    }
        //    return LocalRedirect(Url.Content("~/"));
        //}

        [
            AllowAnonymous,
            HttpGet("isAuthenticated")
        ]
        public IActionResult IsAuthenticated()
            => Json(new { isAuthenticated = User.Identity.IsAuthenticated });

        static AuthenticationProperties CreateAuthenticationProperties(string provider, string redirectUrl)
        {
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items["LoginProvider"] = provider;
            return properties;
        }
    }
}