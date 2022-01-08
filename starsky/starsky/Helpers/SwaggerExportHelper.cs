using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Helpers
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SwaggerExportHelper : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IWebLogger _logger;

		public SwaggerExportHelper(IServiceScopeFactory serviceScopeFactory, IWebLogger logger = null)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
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
				var applicationLifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
				
				Add03AppExport(appSettings, selectorStorage, swaggerProvider);
				Add04SwaggerExportExitAfter(appSettings, applicationLifetime);
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// to run ExecuteAsync from outside this class
		/// </summary>
		/// <returns></returns>
		internal void ExecuteAsync()
		{
			ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
		}
		
		/// <summary>
		/// Export Values to Storage
		/// </summary>
		/// <param name="appSettings">App Settings</param>
		/// <param name="selectorStorage">Storage Provider</param>
		/// <param name="swaggerProvider">Swagger</param>
		/// <exception cref="ArgumentNullException">swaggerJsonText = null</exception>
		public bool Add03AppExport(AppSettings appSettings, ISelectorStorage selectorStorage, ISwaggerProvider swaggerProvider)
		{
			if ( appSettings.AddSwagger != true || appSettings.AddSwaggerExport != true ) return false;
			
			var swaggerJsonText = GenerateSwagger(swaggerProvider, appSettings.Name);
			if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ArgumentException("swaggerJsonText = null", nameof(swaggerProvider));

			var swaggerJsonFullPath =
				Path.Join(appSettings.TempFolder, appSettings.Name.ToLowerInvariant() + ".json");

			var storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			storage.FileDelete(swaggerJsonFullPath);
			storage.WriteStream(new PlainTextFileHelper().StringToStream(swaggerJsonText),
				swaggerJsonFullPath);

			_logger?.LogInformation($"app__addSwaggerExport {swaggerJsonFullPath}");
			return true;
		}

		public bool Add04SwaggerExportExitAfter(AppSettings appSettings, IHostApplicationLifetime applicationLifetime)
		{
			if ( appSettings.AddSwagger == true && appSettings.AddSwaggerExport == true && appSettings.AddSwaggerExportExitAfter == true )
			{
				applicationLifetime.StopApplication();
			}
			return false;
		}

		/// <summary>
		/// Generate Swagger as Json
		/// Set this:
		/// app__AddSwagger = true
		/// app__AddSwaggerExport = true
		/// </summary>
		/// <param name="swaggerProvider">Swagger provider</param>
		/// <param name="docName">document name</param>
		/// <returns></returns>
		internal static string GenerateSwagger(ISwaggerProvider swaggerProvider, string docName)
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
