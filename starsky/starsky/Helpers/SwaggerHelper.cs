using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using starskycore.Helpers;
using starskycore.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace starsky.Helpers
{
	public class SwaggerHelper
	{
		private readonly AppSettings _appSettings;

		public SwaggerHelper(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}


		public void Add01SwaggerGenHelper(IServiceCollection services)
		{
			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(_appSettings.Name, new OpenApiInfo { Title = _appSettings.Name, Version = version });
				c.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme()
				{
					Type = SecuritySchemeType.Http,
					Scheme = "basic",
					Description = "Input your username and password to access this API"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference {
								Type = ReferenceType.SecurityScheme,
								Id = "basicAuth (to logout use the logout api)" }
						}, new List<string>() }
				});
			});
		}

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

		/// <summary>
		///	Currently disabled
		/// </summary>
		/// <param name="app"></param>
		public void Add03AppExport(IApplicationBuilder app)
		{
			if ( !_appSettings.AddSwagger || !_appSettings.AddSwaggerExport ) return;
			using ( var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				.CreateScope() )
			{
				var swaggerJsonText = GenerateSwagger(serviceScope, _appSettings.Name);
				if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ApplicationException("swaggerJsonText = null");

				var starskyJsonPath =
					Path.Join(_appSettings.TempFolder, _appSettings.Name + ".json");
				FilesHelper.DeleteFile(starskyJsonPath);
				new PlainTextFileHelper().WriteFile(starskyJsonPath, swaggerJsonText);
				Console.WriteLine(starskyJsonPath);
			}
		}

		private static string GenerateSwagger(IServiceScope serviceScope, string docName)
		{
			// todo: this feature will break in Swagger 5.x
			// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1152
			var swaggerProvider = ( ISwaggerProvider )serviceScope.ServiceProvider.GetService(typeof(ISwaggerProvider));
			if ( swaggerProvider == null ) return string.Empty;

			var swaggerDocument = swaggerProvider.GetSwagger(docName, null, "/");

			return swaggerDocument.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
		}

		private string GetXmlCommentsPath()
		{
			return Path.Combine(_appSettings.BaseDirectoryProject, "starsky.xml");
		}
	}
}
