using AspNetCore.Proxy;
using VueBffProxy.Server;
using VueBffProxy.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var env = builder.Environment;

services.AddInfrastructure()
    .AddSecurity(configuration);

if (env.IsDevelopment())
{
    services.AddProxies();
}

var app = builder.Build();

//���ð�ȫͷ
app.UseSecurityHeaders(SecurityHeadersDefinitions.GetHeaderPolicyCollection(env.IsDevelopment(), configuration["idp"]));
//ȷ��ÿ��������XSRF-TOKEN
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

// ������SPA�ض�·��

app.MapNotFound("/api/{**segment}");

//������������vite�����ĳ���
if (env.IsDevelopment())
{
    var spaDevServer = app.Configuration.GetValue<string>("SpaDevServerUrl");
    if (!string.IsNullOrEmpty(spaDevServer))
    {
        // ����������ΪӦ�÷��͵�vite-dev���������κ�����
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

// ����spa·��
app.MapFallbackToPage("/_Host");

app.Run();
