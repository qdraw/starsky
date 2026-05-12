using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using starsky.foundation.platform.Models;

namespace starsky.Helpers;

public sealed class SwaggerSetupHelper(AppSettings appSettings)
{
	public void Add01SwaggerGenHelper(IServiceCollection services)
	{
		var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc(appSettings.Name,
				new OpenApiInfo { Title = appSettings.Name, Version = version });
			c.AddSecurityDefinition("basic",
				new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "basic" });
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme, Id = "basic"
						}
					},
					Array.Empty<string>()
				}
			});
			// DescribeAllEnumsAsStrings are not working
			c.IncludeXmlComments(GetXmlCommentsPath());
			c.IncludeXmlComments(GetXmlCloudImportCommentsPath());
		});
	}

	/// <summary>
	///     Expose Swagger to the `/swagger/` endpoint
	/// </summary>
	/// <param name="app"></param>
	public void Add02AppUseSwaggerAndUi(IApplicationBuilder app)
	{
		// Use swagger only when enabled, default false
		// recommend to disable in production
		if ( appSettings == null || appSettings.AddSwagger != true )
		{
			return;
		}

		app.UseSwagger(); // registers the two documents in separate routes
		app.UseSwaggerUI(options =>
		{
			options.DocumentTitle = appSettings.Name;
			options.SwaggerEndpoint("/swagger/" + appSettings.Name + "/swagger.json",
				appSettings.Name);
			options.OAuthAppName(appSettings.Name + " - Swagger");
		}); // makes the ui visible    
	}

	private string GetXmlCommentsPath()
	{
		return Path.Combine(appSettings.BaseDirectoryProject, "starsky.xml");
	}

	private string GetXmlCloudImportCommentsPath()
	{
		return Path.Combine(appSettings.BaseDirectoryProject, "starsky.feature.cloudimport.xml");
	}
}
