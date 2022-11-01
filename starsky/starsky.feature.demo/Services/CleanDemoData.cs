using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.demo.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.demo.Services
{

	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class CleanDemoDataService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private IWebSocketConnectionsService? _connectionsService;
		private INotificationQuery? _notificationQuery;

		public CleanDemoDataService(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Running scoped services
		/// </summary>
		/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
		/// <returns>CompletedTask</returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();
			var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

			if ( Environment.GetEnvironmentVariable("app__storageFolder") == null)
			{
				logger.LogError("[demo mode on] Environment variable app__storageFolder is not set");
				return;
			}

			if (!environment.IsEnvironment("demo") || appSettings.ApplicationType != AppSettings.StarskyAppType.WebController )
			{
				return;
			}
		
			var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
			var sync = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			var subStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

			var httpClientHelper = scope.ServiceProvider.GetRequiredService<IHttpClientHelper>();
			_connectionsService = scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			_notificationQuery = scope.ServiceProvider.GetRequiredService<INotificationQuery>();

			if ( subStorage.ExistFolder("/.stfolder") )
			{
				logger.LogError("stfolder exists so exit");
				return;
			}
			
			subStorage.FolderDelete("/");
			subStorage.CreateDirectory("/");

			await sync.Sync("/",PushToSockets);
			

			Console.WriteLine("download start");
			await Download(appSettings, httpClientHelper,hostStorage,subStorage);
			Console.WriteLine("download done");

			var t = await sync.Sync("/",PushToSockets);

		}
		
		internal async Task PushToSockets(List<FileIndexItem> updatedList)
		{
			var filtered = updatedList.Where(p => p.FilePath != "/").ToList();
			if ( !filtered.Any() )
			{
				return;
			}
			
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(filtered,
					ApiNotificationType.CleanDemoData);
			await _connectionsService!.SendToAllAsync(webSocketResponse, CancellationToken.None);
			await _notificationQuery!.AddNotification(webSocketResponse);
		}
		
		private const string DemoFolderName = "demo";

		private static async Task Download(AppSettings appSettings, IHttpClientHelper httpClientHelper, IStorage hostStorage, IStorage subStorage)
		{

			var cacheFolder = Path.Combine(appSettings.TempFolder, DemoFolderName);
			hostStorage.CreateDirectory(cacheFolder);
			
			foreach ( var (jsonUrl, dir) in appSettings.DemoData )
			{
				var readString = await httpClientHelper.ReadString(jsonUrl);
				if ( !readString.Key ) continue;
				var data = 	JsonSerializer.Deserialize<PublishManifestDemo>(readString.Value);
				if ( data == null ) continue;

				var baseUrl = jsonUrl.Replace("_settings.json", string.Empty); // ends with slash
				
				foreach ( var keyValuePair in data.Copy.Where(p => p.Value && p.Key.Contains("1000")) )
				{
					var fileName =
						FilenamesHelper.GetFileName(keyValuePair.Key);
					var cacheFilePath = Path.Combine(cacheFolder,fileName);

					if ( !hostStorage.ExistFile(cacheFilePath) )
					{
						await httpClientHelper.Download(baseUrl+ keyValuePair.Key,cacheFilePath);
					}

					subStorage.CreateDirectory(dir);
					
					await subStorage.WriteStreamAsync(
							hostStorage.ReadStream(cacheFilePath),
							dir + "/" + fileName);

				}
			}
			
		}
	}
}
