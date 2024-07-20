using Microsoft.AspNetCore.Antiforgery;

namespace VueBffProxy.Server;

//EnsureXSRFToken
public class XSRFTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        
        if (!context.Response.HasStarted)
        {
            var xsrfTokens = antiforgery.GetAndStoreTokens(context);
            var requestToken = xsrfTokens.RequestToken!;

            context.Response.Cookies.Append("XSRF-RequestToken", requestToken, new CookieOptions
            {
                HttpOnly = false, // API调用需要可由JavaScript访问
                Secure = true, // https协议才发送
                SameSite = SameSiteMode.Strict // 防止来自其他站点的CSRF攻击
            });
        }
    }
}
