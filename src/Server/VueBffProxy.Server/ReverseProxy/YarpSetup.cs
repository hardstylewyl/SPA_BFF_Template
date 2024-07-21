using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace VueBffProxy.Server.ReverseProxy
{
	public static class YarpSetup
	{
		public static IServiceCollection ApplyYarp(this IServiceCollection services)
		{
			services.AddReverseProxy()
				.AddTransforms(ctx =>
				{
					ctx.AddPathRemovePrefix("/res").AddPathPrefix("/api");
					ctx.AddRequestTransform(async rctx =>
					{
						var user = rctx.HttpContext.User;
						var token = await rctx.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
						rctx.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					});
				})
				.LoadFromMemory(
				//路由配置
				[
					 new RouteConfig(){
						 RouteId = "res_server",
						 ClusterId = "res_server_cluster",
						 Match = new RouteMatch(){
							 Path = "/res/{**catch-all}",
						 }
					 }
					],
				   //集群配置
				   [
						 new ClusterConfig(){
							 ClusterId = "res_server_cluster",
							 Destinations  = new Dictionary<string,DestinationConfig> {
								 { "node0",new DestinationConfig(){ Address = "https://localhost:7258/" } }
							 }
						 }
						]);

			return services;
		}
	}
}
