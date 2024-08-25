using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace VueBffProxy.Server.Extensions;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddOptions();
		services.AddHttpClient();
		services.AddHttpContextAccessor();

		services.AddControllersWithViews(options =>
			{
				// options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
			});

		services.AddRazorPages();

		services.AddDistributedMemoryCache();

		return services;
	}

	public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
	{
		//增强防护csrf攻击
		services.AddAntiforgery(options =>
		{
			options.HeaderName = "X-XSRF-TOKEN";
			options.Cookie.Name = "__Host-X-XSRF-TOKEN";
			options.Cookie.SameSite = SameSiteMode.Strict;
			options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		});

		//配置鉴权方案
		services.AddAuthentication(options =>
		{
			options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
		})
		.AddCookie()
		.AddOpenIdConnect(o =>
		{
			//登录到Cookies方案，之后可以从cookies方案获取授权的相关信息
			o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			//发行人地址 Issuer/Authority
			o.Authority = configuration["OpenIDConnect:Authority"];
			o.ClientId = configuration["OpenIDConnect:ClientId"];
			o.ClientSecret = configuration["OpenIDConnect:ClientSecret"];
			//当强制启用Https得需要配置证书
			o.RequireHttpsMetadata = true;
			//采用授权码标准流程
			o.ResponseType = OpenIdConnectResponseType.Code;
			o.UsePkce = true;
			o.SaveTokens = true;
			o.GetClaimsFromUserInfoEndpoint = true;
			//所需访问的Scopes,可以在授权服务修改配置
			o.Scope.Add("bff_acceess_scope");

			o.BackchannelHttpHandler = new HttpClientHandler()
			{
				//ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
				ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
				{
					// local dev, just approve all certs
					return true;

					//return errors == SslPolicyErrors.None;
				}
			};
		});

		services.AddAuthorization(options =>
		{
		});

		services.AddHsts(options =>
		{
			options.Preload = true;
			options.IncludeSubDomains = true;
			options.MaxAge = TimeSpan.FromDays(60);
		});

		return services;
	}
}
