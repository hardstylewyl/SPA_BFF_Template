using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
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

#region Openiddict
//配置Openiddict的鉴权方案
services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
services.AddAuthorization();
services.AddOpenIddict()
	.AddValidation(o =>
	{
		//发行 idp
		o.SetIssuer("https://localhost:7051/");
		//受众 自己
		o.AddAudiences("res_server");

		//使用内省端点校验权限
		o.UseIntrospection()
			.SetClientId("res_server")
			.SetClientSecret("res_server_secret");

		o.UseAspNetCore();

		o.UseSystemNetHttp();
	});

#endregion


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
