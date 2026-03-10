using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.import.Helpers;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.import.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImportBackgroundJobHandler(
	ISelectorStorage selectorStorage,
	IServiceScopeFactory scopeFactory,
	IWebLogger logger,
	AppSettings appSettings) : IBackgroundJobHandler
{
	public const string Import = "Import.v1";
	public string JobType => Import;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload");
		}

		var payload = JsonSerializer.Deserialize<ImportBackgroundPayload>(payloadJson)
		              ?? throw new ArgumentException("Invalid payload");

		List<ImportIndexItem> importedFiles;
		using ( var scope = scopeFactory.CreateScope() )
		{
			var localSelectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
			var importQuery = scope.ServiceProvider.GetRequiredService<IImportQuery>();
			var exifTool = scope.ServiceProvider.GetRequiredService<IExifTool>();
			var query = scope.ServiceProvider.GetRequiredService<IQuery>();
			var console = scope.ServiceProvider.GetRequiredService<IConsole>();
			var metaExifThumbnailService =
				scope.ServiceProvider.GetRequiredService<IMetaExifThumbnailService>();
			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var thumbnailQuery = scope.ServiceProvider.GetRequiredService<IThumbnailQuery>();
			var geoCode = scope.ServiceProvider.GetRequiredService<IReverseGeoCodeService>();

			var service = new Import(localSelectorStorage, appSettings,
				importQuery, exifTool, query, console,
				metaExifThumbnailService, logger, thumbnailQuery, geoCode, memoryCache);
			importedFiles = await service.Importer(payload.TempImportPaths, payload.ImportSettings);
		}

		if ( payload.IsVerbose )
		{
			foreach ( var file in importedFiles )
			{
				logger.LogInformation(
					$"[ImportPostBackgroundTask] import {file.Status} => {file.FilePath} ~ {file.FileIndexItem?.FilePath}");
			}
		}

		var hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		foreach ( var toDelPath in payload.TempImportPaths )
		{
			new RemoveTempAndParentStreamFolderHelper(hostFileSystemStorage, appSettings)
				.RemoveTempAndParentStreamFolder(toDelPath);
		}
	}
}
