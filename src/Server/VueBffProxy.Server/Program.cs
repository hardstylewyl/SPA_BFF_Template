using AspNetCore.Proxy;
using VueBffProxy.Server;
using VueBffProxy.Server.Extensions;
using VueBffProxy.Server.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var env = builder.Environment;

services.AddInfrastructure()
	.ApplyYarp()
	.AddSecurity(configuration);

if (env.IsDevelopment())
{
	services.AddProxies();
}

var app = builder.Build();

//配置安全头
app.UseSecurityHeaders(SecurityHeadersDefinitions.GetHeaderPolicyCollection(env.IsDevelopment(), configuration["idp"]));
//确保每次请求都有XSRF-TOKEN
app.UseMiddleware<XSRFTokenMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseNoUnauthorizedRedirect("/api");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// 以下是SPA特定路由

app.MapNotFound("/api/{**segment}");

//开发环境代理vite构建的程序
if (env.IsDevelopment())
{
	var spaDevServer = app.Configuration.GetValue<string>("SpaDevServerUrl");
	if (!string.IsNullOrEmpty(spaDevServer))
	{
		// 代理我们认为应该发送到vite-dev服务器的任何请求
		app.MapWhen(context =>
			{
				var path = context.Request.Path.ToString();
				var isFileRequest = path.StartsWith("/@", StringComparison.InvariantCulture) // some libs
				|| path.StartsWith("/src", StringComparison.InvariantCulture) // source files
				|| path.StartsWith("/node_modules", StringComparison.InvariantCulture); // other libs

				return isFileRequest;
			}, app2 => app2.Run(context =>
			{
				var targetPath = $"{spaDevServer}{context.Request.Path}{context.Request.QueryString}";
				return context.HttpProxyAsync(targetPath);
			}));
	}
}
app.MapReverseProxy();
// 处理spa路由
app.MapFallbackToPage("/_Host");

app.Run();
