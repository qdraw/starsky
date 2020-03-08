using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using starskycore.Helpers;
using starskycore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using starskycore.Interfaces;
using starskycore.Services;

namespace starsky.Helpers
{
	public class SwaggerHelper
	{
		private readonly AppSettings _appSettings;
		private readonly ISelectorStorage _selectorStorage;

		public SwaggerHelper(AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_selectorStorage = selectorStorage;
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

		public void Add03AppExport(IApplicationBuilder app)
		{
			if ( !_appSettings.AddSwagger || !_appSettings.AddSwaggerExport ) return;
			using ( var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				.CreateScope() )
			{
				var swaggerJsonText = GenerateSwagger(serviceScope, _appSettings.Name);
				if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ArgumentNullException(app + " => swaggerJsonText = null");

				var swaggerJsonFullPath =
					Path.Join(_appSettings.TempFolder, _appSettings.Name + ".json");

				var storage = _selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
				storage.FileDelete(swaggerJsonFullPath);
				storage.WriteStream(new PlainTextFileHelper().StringToStream(swaggerJsonText),
					swaggerJsonFullPath);

				if ( _appSettings.Verbose )
				{
					Console.WriteLine($"Add03AppExport {swaggerJsonFullPath}");
				}

			}
		}

		private static string GenerateSwagger(IServiceScope serviceScope, string docName)
		{
			var swaggerProvider = ( ISwaggerProvider )serviceScope.ServiceProvider.GetService(typeof(ISwaggerProvider));
			if ( swaggerProvider == null ) return string.Empty;

			var swaggerDocument = swaggerProvider.GetSwagger(docName, null, "/");
			return JsonConvert.SerializeObject(swaggerDocument, Formatting.Indented,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					ContractResolver = new DefaultContractResolver()
				});
		}


		private string GetXmlCommentsPath()
		{
			return Path.Combine(_appSettings.BaseDirectoryProject, "starsky.xml");
		}
	}
}
