using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using OpeniddictServer.Data;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
services.AddControllersWithViews();
services.AddRazorPages();
services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(2);
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.None;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
// allow all
services.AddCors(a => a.AddDefaultPolicy(builder =>
{
	builder.AllowCredentials()
						   .WithOrigins(
							   "https://localhost:4200")
						   .SetIsOriginAllowedToAllowWildcardSubdomains()
						   .AllowAnyHeader()
						   .AllowAnyMethod();
}));
#region Data

services.AddDatabaseDeveloperPageExceptionFilter();
services.AddDbContext<AppDbContext>(o =>
{
	o.UseOpenIddict();

	o.UseInMemoryDatabase("idp");
});

#endregion


#region Identity

services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders()
	.AddDefaultUI();

services.Configure<IdentityOptions>(o =>
{
	//里面配置一些规则，密码强度/生成密码的规则
	o.Password.RequireDigit = false; //是否必须有数字
	o.Password.RequireLowercase = false; //是否必须有小写字母
	o.Password.RequireNonAlphanumeric = false; //是否必须有除字母数字之外的其他字符
	o.Password.RequireUppercase = false; //是否必须有大写字母
	o.Password.RequiredLength = 6; //最短限制
	o.Lockout.MaxFailedAccessAttempts = 3; //登录失败超过3次触发锁定
	o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //配置锁定时间 5分钟

	//如果配置生成为验证码（较短），若为链接则不用配置此项（生成token较长）
	//获取/配置用于重置密码的令牌为 Email
	o.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
	//获取/配置电子邮件确认令牌的提供程序（邮件激活）
	o.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
	//设置短token
	o.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
});

#endregion


#region Openiddict

//注册Quartz.NET服务并将其配置为在作业完成之前阻止关闭。
services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
services.AddQuartz(options =>
{
	options.UseMicrosoftDependencyInjectionJobFactory();
	options.UseSimpleTypeLoader();
	options.UseInMemoryStore();
});

services.AddOpenIddict()
		  .AddCore(o =>
		  {
			  o.UseEntityFrameworkCore()
				  .UseDbContext<AppDbContext>();

			  o.UseQuartz();
		  })
		  .AddServer(o =>
		  {
			  //支持模式
			  o.AllowAuthorizationCodeFlow()
								   .AllowHybridFlow()
								   .AllowClientCredentialsFlow()
								   .AllowRefreshTokenFlow();

			  //PKCE只在客户端无法保护自己的client_secret的时候使用
			  o.RequireProofKeyForCodeExchange();

			  //配置相关路由
			  //.SetCryptographyEndpointUris("/")
			  //.SetDeviceEndpointUris("connect/device");//设备码授权
			  //.SetConfigurationEndpointUris("server/conf") //获取服务器相关配置默认为.well-known/openid-configuration
			  o.SetAuthorizationEndpointUris("connect/authorize") //授权
				  .SetLogoutEndpointUris("connect/logout") //登出
				  .SetTokenEndpointUris("connect/token") //获取token
				  .SetUserinfoEndpointUris("connect/userinfo") //获取用户信息 
				  .SetRevocationEndpointUris("connect/revoke") //撤销token   
				  .SetIntrospectionEndpointUris("connect/introspect") //内省，验证令牌有效性
				  .SetVerificationEndpointUris("connect/verify"); //验证签名有效性


			  //支持的作用域，如果在此处不指定那么就算client中有此scope也会不允许请求
			  o.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

			  //配置传递模式
			  o.UseAspNetCore()
				  .EnableAuthorizationEndpointPassthrough() //授权
				  .EnableLogoutEndpointPassthrough() //登出
				  .EnableTokenEndpointPassthrough() //获取token
				  .EnableUserinfoEndpointPassthrough() //获取用户信息
				  .EnableStatusCodePagesIntegration(); //启用状态码页面集成支持
													   //不强制使用https
													   //.DisableTransportSecurityRequirement();

			  //配置相关过期时间
			  o.SetAccessTokenLifetime(TimeSpan.FromDays(3)) //配置access_token过期时间
				  .SetAuthorizationCodeLifetime(TimeSpan.FromMinutes(1)) //配置authorization_code过期时间
				  .SetIdentityTokenLifetime(TimeSpan.FromDays(3)) //配置id_token过期时间
				  .SetRefreshTokenLifetime(TimeSpan.FromMinutes(1)) //配置refresh_token过期时间
				  .SetDeviceCodeLifetime(TimeSpan.FromMinutes(1)); //配置device_code过期时间

			  //配置对称加密的密钥
			  //需要在客户端/资源端 以及授权服务端同时使用相同的key
			  //o.AddEncryptionKey(secKey);

			  //注册签名和证书
			  o.AddDevelopmentEncryptionCertificate()
			   .AddDevelopmentSigningCertificate();

		  })
		  .AddValidation(o =>
		  {
			  o.UseLocalServer();
			  o.UseAspNetCore();
		  });

#endregion


// 初始化应用数据
services.AddHostedService<SeedDataHostedService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
