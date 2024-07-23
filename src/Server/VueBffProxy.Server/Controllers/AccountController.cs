using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace VueBffProxy.Server.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{

	[HttpGet]
	public ActionResult Login(string? returnUrl = null)
	{
		return Challenge(new AuthenticationProperties
		{
			RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/"
		});
	}

	[HttpGet]
	public ActionResult Logout()
	{
		return SignOut(new AuthenticationProperties { RedirectUri = "/" },
		 CookieAuthenticationDefaults.AuthenticationScheme,
		 OpenIdConnectDefaults.AuthenticationScheme);
	}

}
