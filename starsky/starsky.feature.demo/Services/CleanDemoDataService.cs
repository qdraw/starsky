using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
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
using starsky.foundation.storage.Helpers;
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
			await RunAsync();
		}

		public async Task<bool?> RunAsync()
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>();

			if (appSettings.DemoUnsafeDeleteStorageFolder != true || appSettings.ApplicationType != AppSettings.StarskyAppType.WebController )
			{
				return false;
			}
			
			if ( Environment.GetEnvironmentVariable("app__storageFolder") == null)
			{
				logger.LogError("[demo mode on] Environment variable app__storageFolder is not set");
				return null;
			}
		
			var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
			var sync = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			var subStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			var httpClientHelper = scope.ServiceProvider.GetRequiredService<IHttpClientHelper>();

			CleanData(subStorage, logger);
			await DownloadAsync(appSettings, httpClientHelper,hostStorage,subStorage, logger);
			await sync.Sync("/",PushToSockets);
			return true;
		}

		public static async Task SeedCli(AppSettings appSettings,
			IHttpClientHelper httpClientHelper, IStorage hostStorage,
			IStorage subStorage, IWebLogger webLogger, ISynchronize sync)
		{
			await DownloadAsync(appSettings, httpClientHelper, hostStorage, subStorage, webLogger);
			await sync.Sync("/");
		}

		internal static void CleanData(IStorage subStorage, IWebLogger logger)
		{
			if ( subStorage.ExistFolder("/.stfolder") )
			{
				logger.LogError("stfolder exists so exit");
				return;
			}
			
			// parent directories
			var directories = subStorage.GetDirectories("/");
			foreach ( var directory in directories )
			{
				subStorage.FolderDelete(directory);
			}
			
			// clean files in root
			var getAllFiles = subStorage.GetAllFilesInDirectory("/")
				.Where(p => p != "/.gitkeep" && p != "/.gitignore").ToList();
			foreach ( var filePath in getAllFiles )
			{
				subStorage.FileDelete(filePath);
			}
		}

		internal async Task<bool> PushToSockets(List<FileIndexItem> updatedList)
		{
			var filtered = updatedList.Where(p => p.FilePath != "/").ToList();
			if ( !filtered.Any() )
			{
				return false;
			}

			using var scope = _serviceScopeFactory.CreateScope();
			var connectionsService = scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			var notificationQuery = scope.ServiceProvider.GetRequiredService<INotificationQuery>();
			
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(filtered,
					ApiNotificationType.CleanDemoData);
			await connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
			await notificationQuery.AddNotification(webSocketResponse);
			return true;
		}
		
		private const string DemoFolderName = "demo";

		
		internal static PublishManifestDemo? Deserialize(string result, IWebLogger webLogger, IStorage hostStorage, string settingsJsonFullPath)
		{
			PublishManifestDemo? data = null;
			try
			{
				data = 	JsonSerializer.Deserialize<PublishManifestDemo?>(result);
			}
			catch ( JsonException exception)
			{
				webLogger.LogError("[Deserialize] catch-ed", exception);
				// and delete to retry
				hostStorage.FileDelete(settingsJsonFullPath);
			}

			return data;
		}
		
		internal static async Task<bool> DownloadAsync(AppSettings appSettings,
			IHttpClientHelper httpClientHelper, IStorage hostStorage,
			IStorage subStorage, IWebLogger webLogger)
		{
			if ( !appSettings.DemoData.Any() )
			{
				webLogger.LogError("DemoData is empty");
				return false;
			}
			
			webLogger.LogInformation("Download demo data");

			var cacheFolder = Path.Combine(appSettings.TempFolder, DemoFolderName);
			hostStorage.CreateDirectory(cacheFolder);
			
			foreach ( var (jsonUrl, dir) in appSettings.DemoData )
			{
				hostStorage.CreateDirectory(Path.Combine(cacheFolder, dir));

				var settingsJsonFullPath =
					Path.Combine(cacheFolder, dir, "_settings.json");
				if ( !hostStorage.ExistFile(settingsJsonFullPath) && !await httpClientHelper.Download(jsonUrl, settingsJsonFullPath))
				{
					webLogger.LogInformation("Skip due not exists: " + settingsJsonFullPath);
					continue;
				}
				
				var result = await PlainTextFileHelper.StreamToStringAsync(
					hostStorage.ReadStream(settingsJsonFullPath));

				var data = Deserialize(result, webLogger, hostStorage, settingsJsonFullPath); 
				if ( data == null )
				{
					webLogger.LogError("[DownloadAsync] data is null");
					continue;
				}

				var baseUrl = jsonUrl.Replace("_settings.json", string.Empty); // ends with slash
				
				foreach ( var keyValuePairKey in data.Copy.Where(p => p.Value 
					         && p.Key.Contains("1000")).Select(p => p.Key) )
				{
					var regex = new Regex("\\?.+$");
					var fileName =
						FilenamesHelper.GetFileName(regex.Replace(keyValuePairKey, string.Empty));
					var cacheFilePath = Path.Combine(cacheFolder,dir, fileName);

					if ( !hostStorage.ExistFile(cacheFilePath) )
					{
						await httpClientHelper.Download(baseUrl+ keyValuePairKey,cacheFilePath);
					}

					subStorage.CreateDirectory(dir);
					
					await subStorage.WriteStreamAsync(
							hostStorage.ReadStream(cacheFilePath),
							PathHelper.AddSlash(dir) + fileName);
				}
			}
			
			webLogger.LogInformation("Demo data seed done");
			return true;
		}
	}
}
