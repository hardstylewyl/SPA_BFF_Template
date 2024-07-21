using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpeniddictServer.Data
{
	public sealed class SeedDataHostedService(IServiceProvider serviceProvider, ILogger<SeedDataHostedService> logger) : IHostedService
	{
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await using var scope = serviceProvider.CreateAsyncScope();
			var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			await context.Database.EnsureCreatedAsync(cancellationToken);

			await CreateAppsAsync(scope.ServiceProvider);
			await CreateScopesAsync(scope.ServiceProvider);


			logger.LogWarning("OpenIddict数据已经初始化完毕");
		}

		private async Task CreateAppsAsync(IServiceProvider provider)
		{
			var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

			// vite应用的bff承载 对应项目 BFF/VueBffProxy.Server
			await manager.CreateAsync(new OpenIddictApplicationDescriptor()
			{
				ClientId = "vite_bff_host",
				ClientSecret = "codeflow_pkce_client_secret",
				ConsentType = ConsentTypes.Explicit,
				DisplayName = "Bff code PKCE",
				PostLogoutRedirectUris =
						{
							new Uri("https://localhost:7017/signout-callback-oidc"),
				},
				RedirectUris =
						{
							new Uri("https://localhost:7017/signin-oidc"),
						},
				Permissions =
						{
							Permissions.Endpoints.Authorization,
							Permissions.Endpoints.Logout,
							Permissions.Endpoints.Token,
							Permissions.Endpoints.Revocation,
							Permissions.GrantTypes.AuthorizationCode,
							Permissions.GrantTypes.RefreshToken,
							Permissions.ResponseTypes.Code,

							Permissions.Scopes.Email,
							Permissions.Scopes.Profile,
							Permissions.Scopes.Roles,


							//#755使用指定的范围，引用范围请移动至#783
							Permissions.Prefixes.Scope + "bff_acceess_scope"
						},
				Requirements =
						{
							Requirements.Features.ProofKeyForCodeExchange
						}
			});


			//资源提供方 对应项目Resource/ResourceServer
			await manager.CreateAsync(new OpenIddictApplicationDescriptor()
			{
				//#652
				ClientId = "res_server",
				ClientSecret = "res_server_secret",
				Permissions =
						{
							Permissions.Endpoints.Introspection
						}
			});


		}

		private async Task CreateScopesAsync(IServiceProvider provider)
		{
			var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

			//scope用于将资源分组
			await manager.CreateAsync(new OpenIddictScopeDescriptor()
			{
				//#783这里的Name唯一标识一个范围，应用使用查看请移至#755
				Name = "bff_acceess_scope",
				DisplayName = "bff_acceess_scope",
				//将放在此分组的资源app的clientId填充于此
				Resources =
				{
					//对应项目Resource/ResourceServer的注册应用id，资源应用查看#652
					"res_server"
				}
			});
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
