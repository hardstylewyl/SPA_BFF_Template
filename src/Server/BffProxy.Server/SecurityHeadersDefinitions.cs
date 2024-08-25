namespace VueBffProxy.Server;

public static class SecurityHeadersDefinitions
{
	//配置 cors和xss 防护
	public static HeaderPolicyCollection GetHeaderPolicyCollection(bool isDev, string? idpHost, bool relaxCspForSwagger = false)
	{
		ArgumentNullException.ThrowIfNull(idpHost);

		var policy = new HeaderPolicyCollection()
			.AddFrameOptionsDeny()
			.AddXssProtectionBlock()
			.AddContentTypeOptionsNoSniff()
			.AddReferrerPolicyStrictOriginWhenCrossOrigin()
			.AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
			.AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
			.AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
			.AddContentSecurityPolicy(builder =>
			{
				builder.AddObjectSrc().None();
				builder.AddBlockAllMixedContent();
				builder.AddImgSrc().Self().From("data:");
				builder.AddFormAction().Self().From(idpHost);
				builder.AddFontSrc().Self().From("data:");

				if (relaxCspForSwagger)
				{
					builder.AddStyleSrc().Self().UnsafeInline();
					builder.AddScriptSrc().Self().UnsafeInline();
				}
				else
				{
					// 配置 csp
					// 笨方法 可以在谷歌浏览器找到控制台报错中提供的sha256值来指定添加
					// 细看到底是 script 还是 style
					builder.AddStyleSrc()
					   .Self()
					   .WithNonce()       //browserLink hashes
					   .WithHash256("47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=")
					   .WithHash256("tVFibyLEbUGj+pO/ZSi96c01jJCvzWilvI5Th+wLeGE=")
					   .WithHash256("F4aWb1J5I2aKKKAQ63BQpSVpWtA+zcxYQc+Im1ijX24=");

					builder.AddScriptSrc()
					 .Self()
					 .WithHash256("eV5p0xsw4UC/bJ48fZ5luze2UmXZbYuQMHs4vAKQynQ=");

					//builder.AddScriptSrc()
					//.WithNonce()
					//.UnsafeInline();
				}

				builder.AddBaseUri().Self();
				builder.AddFrameAncestors().None();
			})
			.RemoveServerHeader()
			.AddPermissionsPolicy(builder =>
			{
				builder.AddAccelerometer().None();
				builder.AddAutoplay().None();
				builder.AddCamera().None();
				builder.AddEncryptedMedia().None();
				builder.AddFullscreen().All();
				builder.AddGeolocation().None();
				builder.AddGyroscope().None();
				builder.AddMagnetometer().None();
				builder.AddMicrophone().None();
				builder.AddMidi().None();
				builder.AddPayment().None();
				builder.AddPictureInPicture().None();
				builder.AddSyncXHR().None();
				builder.AddUsb().None();
			});

		if (!isDev)
		{
			// maxage = one year in seconds
			policy.AddStrictTransportSecurityMaxAgeIncludeSubDomains(60 * 60 * 24 * 365);
		}

		return policy;
	}
}
