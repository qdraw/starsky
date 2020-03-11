using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using starskycore.Models;

namespace starsky.Helpers
{
	public class SwaggerSetupHelper
	{
		private readonly AppSettings _appSettings;

		public SwaggerSetupHelper(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		public void Add01SwaggerGenHelper(IServiceCollection services)
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(_appSettings.Name, new OpenApiInfo { Title = _appSettings.Name, Version = version });
				c.AddSecurityDefinition("basic", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "basic" });
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basic" }
						},
						new string[] {}
					}
				});

				c.IncludeXmlComments(GetXmlCommentsPath());
			});
		}
		
		/// <summary>
		/// Expose Swagger to the `/swagger/` endpoint
		/// </summary>
		/// <param name="app"></param>
		public void Add02AppUseSwaggerAndUi(IApplicationBuilder app)
		{
			// Use swagger only when enabled, default false
			// recommend to disable in production
			if ( !_appSettings.AddSwagger ) return;

			app.UseSwagger(); // registers the two documents in separate routes
			app.UseSwaggerUI(options =>
			{
				options.DocumentTitle = _appSettings.Name;
				options.SwaggerEndpoint("/swagger/" + _appSettings.Name + "/swagger.json", _appSettings.Name);
				options.OAuthAppName(_appSettings.Name + " - Swagger");
			}); // makes the ui visible    
		}
		
		private string GetXmlCommentsPath()
		{
			return Path.Combine(_appSettings.BaseDirectoryProject, "starsky.xml");
		}
	}
}
