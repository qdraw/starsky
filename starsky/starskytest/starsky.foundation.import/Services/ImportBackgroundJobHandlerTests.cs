using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class ImportBackgroundJobHandlerTests
{
	[TestMethod]
	public void JobType_ReturnsCorrectValue()
	{
		var handler = new ImportBackgroundJobHandler(null!, null!, new AppSettings());
		Assert.AreEqual(ImportBackgroundJobHandler.Import, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_NullPayload_ThrowsArgumentException()
	{
		var handler = new ImportBackgroundJobHandler(null!, null!, new AppSettings());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(null, CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_EmptyPayload_ThrowsArgumentException()
	{
		var handler = new ImportBackgroundJobHandler(null!, null!, new AppSettings());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync(" ", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ThrowsJsonException()
	{
		var handler = new ImportBackgroundJobHandler(null!, null!, new AppSettings());
		await Assert.ThrowsExactlyAsync<JsonException>(() =>
			handler.ExecuteAsync("invalid-json", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_NullDeserialized_ThrowsArgumentException()
	{
		var handler = new ImportBackgroundJobHandler(null!, null!, new AppSettings());
		await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			handler.ExecuteAsync("null", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_ValidPayload_CallsImportPostBackgroundTask()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<IImportQuery, FakeIImportQuery>();
		services.AddSingleton<IExifTool, FakeExifTool>();
		services.AddSingleton<IQuery, FakeIQuery>();
		services.AddSingleton<IImport, FakeIImport>();
		services.AddSingleton<IConsole, FakeConsoleWrapper>();
		services.AddSingleton<IThumbnailQuery, FakeIThumbnailQuery>();
		services.AddSingleton<IMetaExifThumbnailService, FakeIMetaExifThumbnailService>();
		services.AddSingleton<IReverseGeoCodeService, FakeIReverseGeoCodeService>();
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();
		services.AddMemoryCache();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var handler = new ImportBackgroundJobHandler(scopeFactory, new FakeIWebLogger(), new AppSettings());

		var payload = new ImportBackgroundPayload
		{
			TempImportPaths = ["/test"],
			ImportSettings = new ImportSettingsModel()
		};
		var json = JsonSerializer.Serialize(payload);

		await handler.ExecuteAsync(json, CancellationToken.None);

		// Verification is implicit here, it shouldn't throw.
		// Detailed verification of ImportPostBackgroundTask is in other tests.
	}

	[TestMethod]
	public async Task ImportPostBackgroundTask_NotFound()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<IImportQuery, FakeIImportQuery>();
		services.AddSingleton<IExifTool, FakeExifTool>();
		services.AddSingleton<IQuery, FakeIQuery>();
		services.AddSingleton<IImport, FakeIImport>();
		services.AddSingleton<IConsole, FakeConsoleWrapper>();
		services.AddSingleton<IThumbnailQuery, FakeIThumbnailQuery>();
		services.AddSingleton<IMetaExifThumbnailService, FakeIMetaExifThumbnailService>();
		services.AddSingleton<IReverseGeoCodeService, FakeIReverseGeoCodeService>();

		// metrics
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();
		services.AddMemoryCache();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var importController =
			new ImportBackgroundJobHandler(scopeFactory, new FakeIWebLogger(), new AppSettings());

		var result = await importController.ImportPostBackgroundTask(
			["/test"], new ImportSettingsModel());

		Assert.HasCount(1, result);
		Assert.AreEqual(ImportStatus.NotFound, result[0].Status);
	}

	[TestMethod]
	public async Task ImportPostBackgroundTask_NotFound_Logger_Contain1()
	{
		var services = new ServiceCollection();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<AppSettings>();
		services.AddSingleton<IImportQuery, FakeIImportQuery>();
		services.AddSingleton<IExifTool, FakeExifTool>();
		services.AddSingleton<IQuery, FakeIQuery>();
		services.AddSingleton<IImport, FakeIImport>();
		services.AddSingleton<IConsole, FakeConsoleWrapper>();
		services.AddSingleton<IThumbnailQuery, FakeIThumbnailQuery>();
		services.AddSingleton<IMetaExifThumbnailService, FakeIMetaExifThumbnailService>();
		services.AddSingleton<IReverseGeoCodeService, FakeIReverseGeoCodeService>();
		// metrics
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		services.AddMemoryCache();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var logger = new FakeIWebLogger();
		var importController =
			new ImportBackgroundJobHandler(scopeFactory, logger, new AppSettings());

		await importController.ImportPostBackgroundTask(
			["/test"], new ImportSettingsModel(), true);

		Assert.HasCount(1, logger.TrackedInformation);
	}
}
