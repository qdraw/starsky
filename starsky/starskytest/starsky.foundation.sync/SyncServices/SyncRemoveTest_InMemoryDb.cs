using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices;

[TestClass]
public sealed class SyncRemoveTestInMemoryDb
{
	private readonly AppSettings _appSettings;
	private readonly IQuery _query;

	public SyncRemoveTestInMemoryDb()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache();

		_appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			Verbose = true,
			DatabaseConnection = nameof(SyncRemoveTestInMemoryDb)
		};

		provider.AddSingleton(_appSettings);

		new SetupDatabaseTypes(_appSettings, provider).BuilderDb();
		provider.AddScoped<IQuery, Query>();
		provider.AddScoped<IWebLogger, FakeIWebLogger>();

		var serviceProvider = provider.BuildServiceProvider();

		_query = serviceProvider.GetRequiredService<IQuery>();
	}


	[TestMethod]
	public async Task Remove_Folder_With_ChildItems()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/Folder_With_ChildItems") { IsDirectory = true },
			new("/Folder_With_ChildItems/test.jpg"),
			new("/Folder_With_ChildItems/test2.jpg")
		});

		var syncRemove = new SyncRemove(_appSettings, _query,
			new FakeMemoryCache(), new FakeIWebLogger(), null);
		var result = await syncRemove.RemoveAsync("/Folder_With_ChildItems");

		Assert.HasCount(3, result);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[1].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[2].Status);
		Assert.AreEqual("/Folder_With_ChildItems", result[0].FilePath);
		Assert.AreEqual("/Folder_With_ChildItems/test.jpg", result[1].FilePath);
		Assert.AreEqual("/Folder_With_ChildItems/test2.jpg", result[2].FilePath);
	}
}
