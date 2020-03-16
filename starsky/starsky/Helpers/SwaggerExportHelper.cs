using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using starskycore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using starsky.foundation.injection;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Helpers
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SwaggerExportHelper : BackgroundService, IHostedService
	{
		public IServiceScopeFactory _serviceScopeFactory;
		
		public SwaggerExportHelper(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}
		
		/// <summary>
		/// Running scoped services
		/// @see: https://thinkrethink.net/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
		/// </summary>
		/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
		/// <returns>CompletedTask</returns>
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
				var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
				var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
				Add03AppExport(appSettings, selectorStorage, swaggerProvider);
			}
			return Task.CompletedTask;
		}
		
		/// <summary>
		/// Export Values to Storage
		/// </summary>
		/// <param name="appSettings">App Settings</param>
		/// <param name="selectorStorage">Storage Provider</param>
		/// <param name="swaggerProvider">Swagger</param>
		/// <exception cref="ArgumentNullException">swaggerJsonText = null</exception>
		public void Add03AppExport(AppSettings appSettings, ISelectorStorage selectorStorage, ISwaggerProvider swaggerProvider)
		{
			if ( !appSettings.AddSwagger || !appSettings.AddSwaggerExport ) return;
			
			var swaggerJsonText = GenerateSwagger(swaggerProvider, appSettings.Name);
			if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ArgumentNullException("swaggerJsonText = null");

			var swaggerJsonFullPath =
				Path.Join(appSettings.TempFolder, appSettings.Name.ToLowerInvariant() + ".json");

			var storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			storage.FileDelete(swaggerJsonFullPath);
			storage.WriteStream(new PlainTextFileHelper().StringToStream(swaggerJsonText),
				swaggerJsonFullPath);

			if ( appSettings.Verbose ) Console.WriteLine($"app__addSwaggerExport {swaggerJsonFullPath}");
		}

		/// <summary>
		/// Generate Swagger as Json
		/// </summary>
		/// <param name="swaggerProvider">Swagger provider</param>
		/// <param name="docName">document name</param>
		/// <returns></returns>
		private static string GenerateSwagger(ISwaggerProvider swaggerProvider, string docName)
		{
			if ( swaggerProvider == null ) return string.Empty;

			var swaggerDocument = swaggerProvider.GetSwagger(docName, null, "/");
			return JsonConvert.SerializeObject(swaggerDocument, Formatting.Indented,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					ContractResolver = new DefaultContractResolver()
				});
		}
	}
}
