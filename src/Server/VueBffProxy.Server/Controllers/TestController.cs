using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VueBffProxy.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController(IAntiforgery antiforgery) : ControllerBase
{
	[HttpGet]
	public IActionResult Get()
	{
		return Ok("Hello");
	}

	[HttpGet]
	public IActionResult CheckAuth()
	{
		var user = HttpContext.User.Identity?.IsAuthenticated ?? false;
		return Ok(user);
	}

	//[ValidateAntiForgeryToken]
	[HttpPost]
	public async Task<IActionResult> Sigin([FromQuery] string name, [FromQuery] string? returnUrl = null)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
			new(ClaimTypes.Name, name),
			new(ClaimTypes.Role, "User")
		};

		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		var principal = new ClaimsPrincipal(identity);

		var props = new AuthenticationProperties
		{
			RedirectUri = returnUrl ?? "/",
			AllowRefresh = true,
			IsPersistent = true
		};

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			principal,
			props);

		return Ok();
	}

	[ValidateAntiForgeryToken]
	[Authorize]
	[HttpPost]
	public async Task<IActionResult> Sigout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		// 用户登出即代表会话结束，需要清理浏览器的XSRF令牌
		Response.Cookies.Delete("XSRF-RequestToken");

		return Ok();
	}

	[Authorize]
	[HttpGet]
	public IActionResult UserInfo()
	{
		var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
		var name = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)!.Value;

		// 当用户登录时，生成并存储一个新的XSRF令牌。
		// 这个令牌会与当前用户进行关联，即User对象 ClaimsPrincipal类型
		// 此过程可以单独抽出一个接口用于刷新token，这里简化在获取用户信息时更新XSRF Token
		// 可以指定一个策略，经过指定的时间间隔将更新XSRF Token
		var xsrfTokens = antiforgery.GetAndStoreTokens(HttpContext);
		var requestToken = xsrfTokens.RequestToken;

		Response.Cookies.Append("XSRF-RequestToken", requestToken!, new CookieOptions
		{
			HttpOnly = false, // API调用需要可由JavaScript访问
			Secure = true, // https协议才发送
			SameSite = SameSiteMode.Strict // 防止来自其他站点的CSRF攻击
		});
		return Ok(new UserInfo(id, name));
	}
}

public record UserInfo(string Id, string Name);
