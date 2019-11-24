using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using starskycore.Helpers;
using starskycore.Models;
using Newtonsoft.Json;

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
//			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
//
//			services.AddSwaggerGen(c =>
//			{
//				c.SwaggerDoc(_appSettings.Name, new Info { Title = _appSettings.Name, Version = version });
//				c.AddSecurityDefinition("basic", new BasicAuthScheme { Type = "basic", Description = "basic authentication" });
//				c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "basic", new string[] { } }, });
//
//				c.IncludeXmlComments(GetXmlCommentsPath());
//				c.DescribeAllEnumsAsStrings();
//
//				// todo: break in Swagger 5.x
//				c.DocumentFilter<BasicAuthFilter>();
//			});

		}

		public void Add02AppUseSwaggerAndUi(IApplicationBuilder app)
		{
			// Use swagger only when enabled, default false
			// recommend to disable in production
			if ( !_appSettings.AddSwagger ) return;
//
//			app.UseSwagger(); // registers the two documents in separate routes
//			app.UseSwaggerUI(options =>
//			{
//				options.DocumentTitle = _appSettings.Name;
//				options.SwaggerEndpoint("/swagger/" + _appSettings.Name + "/swagger.json", _appSettings.Name);
//				options.OAuthAppName(_appSettings.Name + " - Swagger");
//			}); // makes the ui visible    
		}

		public void Add03AppExport(IApplicationBuilder app)
		{
			if ( !_appSettings.AddSwagger || !_appSettings.AddSwaggerExport ) return;
//			using ( var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
//				.CreateScope() )
//			{
//				var swaggerJsonText = GenerateSwagger(serviceScope, _appSettings.Name);
//				if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ArgumentNullException(app + " => swaggerJsonText = null");
//
//				var starskyJsonPath =
//					Path.Join(_appSettings.TempFolder, _appSettings.Name + ".json");
//				FilesHelper.DeleteFile(starskyJsonPath);
//				new PlainTextFileHelper().WriteFile(starskyJsonPath, swaggerJsonText);
//				Console.WriteLine(starskyJsonPath);
//			}
		}

//		private static string GenerateSwagger(IServiceScope serviceScope, string docName)
//		{
//			// todo: this feature will break in Swagger 5.x
//			var swaggerProvider = ( ISwaggerProvider )serviceScope.ServiceProvider.GetService(typeof(ISwaggerProvider));
//			if ( swaggerProvider == null ) return string.Empty;
//
//			var swaggerDocument = swaggerProvider.GetSwagger(docName, null, "/");
//			return JsonConvert.SerializeObject(swaggerDocument, Formatting.Indented,
//				new JsonSerializerSettings
//				{
//					NullValueHandling = NullValueHandling.Ignore,
//					ContractResolver = new SwaggerContractResolver(new JsonSerializerSettings())
//				});
//		}
//
//		// todo: this feature will break in Swagger 5.x
//		private class BasicAuthFilter : IDocumentFilter
//		{
//			public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
//			{
//				var securityRequirements = new Dictionary<string, IEnumerable<string>>()
//				{
//					{ "basic", new string[] { } }
//				};
//
//				swaggerDoc.Security = new IDictionary<string, IEnumerable<string>>[] { securityRequirements };
//			}
//		}

		private string GetXmlCommentsPath()
		{
			return Path.Combine(_appSettings.BaseDirectoryProject, "starsky.xml");
		}
	}
}
