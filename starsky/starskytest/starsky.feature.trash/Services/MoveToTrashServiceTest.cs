using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.feature.trash.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.native.Trash;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.trash.Services;

[TestClass]
public class MoveToTrashServiceTest
{
	[TestMethod]
	public async Task InSystemTrash()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = true };
		var moveToTrashService = new MoveToTrashService(appSettings, new FakeIQuery(new List<FileIndexItem>{new FileIndexItem(path)
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}), 
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(), 
			trashService, new FakeIMetaUpdateService(), 
			new FakeITrashConnectionService());

		await moveToTrashService.MoveToTrashAsync(new List<string>{path}.ToArray(), true);
		
		Assert.AreEqual(1, trashService.InTrash.Count);
		var expected = appSettings.StorageFolder +
			path.Replace('/', Path.DirectorySeparatorChar);
		Assert.AreEqual(expected, trashService.InTrash.FirstOrDefault());
	}
	
	[TestMethod]
	public async Task InMetaTrash_StatusOk_IsNotSupported_AndEnabled()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService(){IsSupported = false};
		var appSettings = new AppSettings { UseSystemTrash = true }; // see supported
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings, new FakeIQuery(new List<FileIndexItem>{new FileIndexItem(path)
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}), 
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(), 
			trashService, metaUpdate, 
			new FakeITrashConnectionService());

		var result = await moveToTrashService.MoveToTrashAsync(new List<string>{path}.ToArray(), true);
		
		Assert.AreEqual(0, trashService.InTrash.Count);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}
	
	[TestMethod]
	public async Task InMetaTrash_StatusOk_IsSupported_AndDisabled()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService(){IsSupported = true};
		var appSettings = new AppSettings { UseSystemTrash = false }; // see supported and other test
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings, new FakeIQuery(new List<FileIndexItem>{new FileIndexItem(path)
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}), 
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(), 
			trashService, metaUpdate, 
			new FakeITrashConnectionService());

		var result = await moveToTrashService.MoveToTrashAsync(new List<string>{path}.ToArray(), true);
		
		Assert.AreEqual(0, trashService.InTrash.Count);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}
	
	[TestMethod]
	public async Task InMetaTrash_StatusDeleted()
	{
		const string path = "/test/test.jpg";
		var trashService = new FakeITrashService(){IsSupported = false};
		var appSettings = new AppSettings { UseSystemTrash = true }; // see supported
		var metaUpdate = new FakeIMetaUpdateService();
		var moveToTrashService = new MoveToTrashService(appSettings, new FakeIQuery(new List<FileIndexItem>{new FileIndexItem(path)
			{
				Status = FileIndexItem.ExifStatus.Deleted
			}}), 
			new FakeMetaPreflight(), new FakeIUpdateBackgroundTaskQueue(), 
			trashService, metaUpdate, 
			new FakeITrashConnectionService());

		var result = await moveToTrashService.MoveToTrashAsync(new List<string>{path}.ToArray(), true);
		
		Assert.AreEqual(0, trashService.InTrash.Count);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}
	
	[TestMethod]
	public async Task InMetaTrash_WithDbContext()
	{
		const string path = "/test/test.jpg";
		
		var trashService = new FakeITrashService(){IsSupported = false};
		var appSettings = new AppSettings { UseSystemTrash = false, DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase}; // see supported
		
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(MoveToTrashServiceTest));
		var options = builderDb.Options;
		var dbContext = new ApplicationDbContext(options);

		var serviceCollection = new ServiceCollection().AddScoped(provider => new ApplicationDbContext(options));
		var serviceScopeFactory = serviceCollection.BuildServiceProvider().GetService<IServiceScopeFactory>();
		
		var storage = new FakeIStorage(new List<string>{"/", "/test"}, new List<string>{path});

		var query = new Query(dbContext, appSettings, serviceScopeFactory,
			new FakeIWebLogger());
		var addedItem = await query.AddItemAsync(new FileIndexItem(path){Id = 9000});
		
		var metaUpdate = new MetaUpdateService(query, new FakeExifTool(storage, appSettings), 
			new FakeSelectorStorage(storage), new MetaPreflight(query, appSettings, new FakeSelectorStorage(storage), 
				new FakeIWebLogger()), new FakeIWebLogger(), new ReadMetaSubPathStorage(new FakeSelectorStorage(storage), 
				appSettings, null, new FakeIWebLogger()), new FakeIThumbnailService(), 
			new ThumbnailQuery(dbContext,null));

		var metaPreflight = new MetaPreflight(query, appSettings,
			new FakeSelectorStorage(storage), new FakeIWebLogger());
		
		var moveToTrashService = new MoveToTrashService(appSettings, query,
			metaPreflight, new FakeIUpdateBackgroundTaskQueue(),
			new TrashService(), metaUpdate, new FakeITrashConnectionService());

		var result = await moveToTrashService.MoveToTrashAsync(new List<string>{path}.ToArray(), true);

		await query.RemoveItemAsync(addedItem);
		
		Assert.AreEqual(0, trashService.InTrash.Count);

		Assert.AreEqual(TrashKeyword.TrashKeywordString, result.FirstOrDefault()?.Tags);
	}
}