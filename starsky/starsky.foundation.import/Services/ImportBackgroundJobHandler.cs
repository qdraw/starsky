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

		await ImportPostBackgroundTask(payload.TempImportPaths, payload.ImportSettings);
	}

	internal async Task<List<ImportIndexItem>> ImportPostBackgroundTask(
		List<string> tempImportPaths,
		ImportSettingsModel importSettings, bool isVerbose = false)
	{
		using var scope = scopeFactory.CreateScope();
		var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
		var importQuery = scope.ServiceProvider.GetRequiredService<IImportQuery>();
		var exifTool = scope.ServiceProvider.GetRequiredService<IExifTool>();
		var query = scope.ServiceProvider.GetRequiredService<IQuery>();
		var console = scope.ServiceProvider.GetRequiredService<IConsole>();
		var metaExifThumbnailService =
			scope.ServiceProvider.GetRequiredService<IMetaExifThumbnailService>();
		var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
		var thumbnailQuery = scope.ServiceProvider.GetRequiredService<IThumbnailQuery>();
		var geoCode = scope.ServiceProvider.GetRequiredService<IReverseGeoCodeService>();

		// use of IImport direct does not work
		var service = new Import(selectorStorage, appSettings,
			importQuery, exifTool, query, console,
			metaExifThumbnailService, logger, thumbnailQuery, geoCode, memoryCache);
		var importedFiles = await service.Importer(tempImportPaths, importSettings);

		if ( isVerbose )
		{
			foreach ( var file in importedFiles )
			{
				logger.LogInformation(
					$"[ImportPostBackgroundTask] import {file.Status} " +
					$"=> {file.FilePath} ~ {file.FileIndexItem?.FilePath}");
			}
		}

		var hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

		// Remove source files
		foreach ( var toDelPath in tempImportPaths )
		{
			new RemoveTempAndParentStreamFolderHelper(hostFileSystemStorage, appSettings)
				.RemoveTempAndParentStreamFolder(toDelPath);
		}

		return importedFiles;
	}
}
