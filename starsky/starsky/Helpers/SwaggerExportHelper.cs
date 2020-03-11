using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using starskycore.Helpers;
using starskycore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using starsky.foundation.injection;
using starskycore.Interfaces;
using starskycore.Services;

namespace starsky.Helpers
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SwaggerExportHelper : BackgroundService
	{
		private readonly AppSettings _appSettings;
		private readonly ISelectorStorage _selectorStorage;
		private readonly ISwaggerProvider _swaggerProvider;

		public SwaggerExportHelper(AppSettings appSettings, ISelectorStorage selectorStorage, ISwaggerProvider swaggerProvider)
		{
			_appSettings = appSettings;
			_selectorStorage = selectorStorage;
			_swaggerProvider = swaggerProvider;
		}
		
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Add03AppExport();
			return Task.CompletedTask;
		}
		
		/// <summary>
		/// Export Values to Storage
		/// </summary>
		/// <param name="app"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void Add03AppExport()
		{
			if ( !_appSettings.AddSwagger || !_appSettings.AddSwaggerExport ) return;
			
			var swaggerJsonText = GenerateSwagger(_swaggerProvider, _appSettings.Name);
			if ( string.IsNullOrEmpty(swaggerJsonText) ) throw new ArgumentNullException("swaggerJsonText = null");

			var swaggerJsonFullPath =
				Path.Join(_appSettings.TempFolder, _appSettings.Name.ToLowerInvariant() + ".json");

			var storage = _selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			storage.FileDelete(swaggerJsonFullPath);
			storage.WriteStream(new PlainTextFileHelper().StringToStream(swaggerJsonText),
				swaggerJsonFullPath);

			if ( _appSettings.Verbose )
			{
				Console.WriteLine($"app__addSwaggerExport {swaggerJsonFullPath}");
			}
		}

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
