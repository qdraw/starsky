using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

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
}
