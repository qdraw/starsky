using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Interfaces;
using starsky.feature.trash.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.native.Trash;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.worker.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.trash.Services;

[TestClass]
public class MoveToTrashServiceTest
{
	private static IServiceScopeFactory CreateServiceScope()
	{
		var serviceProvider = new ServiceCollection()
			.AddSingleton<AppSettings>()
			.AddSingleton<ITrashService, FakeITrashService>()
			.AddSingleton<IQuery, FakeIQuery>()
			.AddSingleton<IUpdateBackgroundTaskQueue, FakeIUpdateBackgroundTaskQueue>()
			.AddSingleton<IMetaPreflight, FakeMetaPreflight>()
			.AddSingleton<IMetaUpdateService, FakeIMetaUpdateService>()
			.AddSingleton<ITrashConnectionService, TrashConnectionService>()
			.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>()
			.AddSingleton<INotificationQuery, FakeINotificationQuery>()
			.AddSingleton<IMoveToTrashService, MoveToTrashService>()
			.AddSingleton<IBackgroundJobHandler, MoveToTrashJobHandler>()
			.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task InSystemTrash_ShouldMoveToTrash()
	{
		var scopeFactory = CreateServiceScope();
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = true };
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Ok }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(scopeFactory),
			trashService, new FakeIMetaUpdateService(),
			new FakeITrashConnectionService());

		await moveToTrashService.CreateEvent([path], true);

		var scope = scopeFactory.CreateScope();
		var trashService2 = scope.ServiceProvider.GetService<ITrashService>() as FakeITrashService;
		Assert.IsNotNull(trashService2);

		Assert.HasCount(1, trashService2.InTrash);
		var expected = appSettings.StorageFolder +
		               path.Replace('/', Path.DirectorySeparatorChar);
		Assert.AreEqual(expected, trashService2.InTrash.FirstOrDefault());
	}

	[TestMethod]
	public async Task InSystemTrash_ShouldMoveToTrash_Directory()
	{
		var scopeFactory = CreateServiceScope();
		const string dirPath = "/test";
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = true };
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([
				new FileIndexItem(path)
				{
					IsDirectory = false, Status = FileIndexItem.ExifStatus.Ok
				},
				new FileIndexItem(dirPath)
				{
					IsDirectory = true, Status = FileIndexItem.ExifStatus.Ok
				}
			]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(scopeFactory),
			trashService, new FakeIMetaUpdateService(),
			new FakeITrashConnectionService());

		await moveToTrashService.CreateEvent([dirPath], true);
		
		var scope = scopeFactory.CreateScope();
		var trashService2 = scope.ServiceProvider.GetService<ITrashService>() as FakeITrashService;
		Assert.IsNotNull(trashService2);

		Assert.HasCount(1, trashService2.InTrash);
		var expected = appSettings.StorageFolder +
		               dirPath.Replace('/', Path.DirectorySeparatorChar);
		Assert.AreEqual(expected, trashService2.InTrash.FirstOrDefault());
	}

	[TestMethod]
	public async Task InSystemTrash_ShouldMoveToTrash_Status()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = true };
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Ok }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, new FakeIMetaUpdateService(),
			new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task InMetaTrash_Status()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = false };
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Ok }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, new FakeIMetaUpdateService(),
			new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task InMetaTrash_StatusOk_IsNotSupported_AndEnabled()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService { IsSupported = false };
		var appSettings = new AppSettings { UseSystemTrash = true }; // see supported
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Ok }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, metaUpdate,
			new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		Assert.IsEmpty(trashService.InTrash);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public async Task InMetaTrash_StatusOk_IsSupported_AndDisabled()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService { IsSupported = true };
		var appSettings =
			new AppSettings { UseSystemTrash = false }; // see supported and other test
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Ok }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, metaUpdate,
			new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		Assert.IsEmpty(trashService.InTrash);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public async Task InMetaTrash_StatusDeleted()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService { IsSupported = false };
		var appSettings = new AppSettings { UseSystemTrash = true }; // see supported
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Deleted }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, metaUpdate,
			new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		Assert.IsEmpty(trashService.InTrash);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public async Task InMetaTrash_WithDbContext()
	{
		const string path = "/test/test.jpg";

		var trashService = new FakeITrashService { IsSupported = false };
		var appSettings = new AppSettings
		{
			UseSystemTrash = false, DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}; // see supported

		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(MoveToTrashServiceTest));
		var options = builderDb.Options;
		var dbContext = new ApplicationDbContext(options);

		var serviceCollection =
			new ServiceCollection().AddScoped(_ => new ApplicationDbContext(options));
		var serviceScopeFactory =
			serviceCollection.BuildServiceProvider().GetService<IServiceScopeFactory>();

		var storage = new FakeIStorage(
			["/", "/test"],
			[path]
		);

		var query = new Query(dbContext, appSettings, serviceScopeFactory,
			new FakeIWebLogger());
		var addedItem = await query.AddItemAsync(new FileIndexItem(path) { Id = 9000 });

		var metaUpdate = new MetaUpdateService(query, new FakeExifTool(storage, appSettings),
			new FakeSelectorStorage(storage), new MetaPreflight(query, appSettings,
				new FakeSelectorStorage(storage),
				new FakeIWebLogger()), new FakeIWebLogger(), new ReadMetaSubPathStorage(
				new FakeSelectorStorage(storage),
				appSettings, new FakeIWebLogger()), new FakeIThumbnailService(),
			new ThumbnailQuery(dbContext, null, new FakeIWebLogger(),
				new FakeMemoryCache()), new AppSettings());

		var metaPreflight = new MetaPreflight(query, appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger());

		var moveToTrashService = new MoveToTrashService(appSettings, query,
			metaPreflight, new FakeIUpdateBackgroundTaskQueue(),
			new TrashService(), metaUpdate, new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		await query.RemoveItemAsync(addedItem);

		Assert.IsEmpty(trashService.InTrash);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}

	[TestMethod]
	public async Task InMetaTrash_WithDbContext_Directory()
	{
		const string path = "/test";
		const string childItem = "/test/test.jpg";

		var trashService = new FakeITrashService { IsSupported = false };
		var appSettings = new AppSettings
		{
			UseSystemTrash = false, DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}; // see supported

		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(MoveToTrashServiceTest));
		var options = builderDb.Options;
		var dbContext = new ApplicationDbContext(options);

		var serviceCollection =
			new ServiceCollection().AddScoped(_ => new ApplicationDbContext(options));
		var serviceScopeFactory =
			serviceCollection.BuildServiceProvider().GetService<IServiceScopeFactory>();

		var storage = new FakeIStorage(
			["/", "/test"],
			[path]
		);

		var query = new Query(dbContext, appSettings, serviceScopeFactory,
			new FakeIWebLogger());
		var addedItem = await query.AddRangeAsync([
			new FileIndexItem(path) { Id = 8830, IsDirectory = true },
			new FileIndexItem(childItem) { Id = 8831 }
		]);
		Console.WriteLine("add done");

		var metaUpdate = new MetaUpdateService(query, new FakeExifTool(storage, appSettings),
			new FakeSelectorStorage(storage), new MetaPreflight(query, appSettings,
				new FakeSelectorStorage(storage),
				new FakeIWebLogger()), new FakeIWebLogger(), new ReadMetaSubPathStorage(
				new FakeSelectorStorage(storage),
				appSettings, new FakeIWebLogger()), new FakeIThumbnailService(),
			new ThumbnailQuery(dbContext, null,
				new FakeIWebLogger(), new FakeMemoryCache()), new AppSettings());

		var metaPreflight = new MetaPreflight(query, appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger());

		var moveToTrashService = new MoveToTrashService(appSettings, query,
			metaPreflight, new FakeIUpdateBackgroundTaskQueue(),
			new TrashService(), metaUpdate, new FakeITrashConnectionService());

		var result = await moveToTrashService.CreateEvent(
			[path], true);

		await query.RemoveItemAsync(addedItem);

		// not in system trash
		Assert.IsEmpty(trashService.InTrash);

		// result
		Assert.HasCount(2, result);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result[0].Tags);
		Assert.AreEqual(TrashKeyword.TrashKeywordString, result[1].Tags);
	}

	[TestMethod]
	public void DetectToUseSystemTrash_False()
	{
		var trashService = new FakeITrashService { IsSupported = false };
		var moveToTrashService = new MoveToTrashService(new AppSettings(), new FakeIQuery(),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, new FakeIMetaUpdateService(),
			new FakeITrashConnectionService());

		// used for end2end test to enable / disable the trash
		var result = moveToTrashService.DetectToUseSystemTrash();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task AppendChildItemsToTrashList_NoAny()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService { IsSupported = false };
		var appSettings = new AppSettings { UseSystemTrash = true }; // see supported
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings,
			new FakeIQuery([new FileIndexItem(path) { Status = FileIndexItem.ExifStatus.Deleted }]),
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(),
			trashService, metaUpdate,
			new FakeITrashConnectionService());

		var (fileIndexResultsList, _) = await moveToTrashService.AppendChildItemsToTrashList(
			[new FileIndexItem("")],
			new Dictionary<string, List<string>>());

		Assert.AreEqual(FileIndexItem.ExifStatus.Default,
			fileIndexResultsList.FirstOrDefault()?.Status);
	}
}
