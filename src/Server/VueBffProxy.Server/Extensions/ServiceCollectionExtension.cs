using Microsoft.AspNetCore.Mvc;

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

        services.AddAuthentication()
            .AddCookie();

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
