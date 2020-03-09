using System.IO;
using System.Reflection;
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
		
		private string GetXmlCommentsPath()
		{
			return Path.Combine(_appSettings.BaseDirectoryProject, "starsky.xml");
		}
	}
}
