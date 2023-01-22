using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.import.Helpers;

[TestClass]
public class RemoveTempAndParentStreamFolderHelperTest
{
	[TestMethod]
	public void RemoveTempAndParentStreamFolderHelper_CanNotRemoveRootOfFileSystem()
	{
		var appSettings = new AppSettings();
		
		var rootFolder = "/";
		if ( appSettings.IsWindows )
		{
			rootFolder = "C:\\";
		}

		var storage = new FakeIStorage(new List<string>{rootFolder});
		new RemoveTempAndParentStreamFolderHelper(storage, appSettings).RemoveTempAndParentStreamFolder(rootFolder);
		
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, storage.Info(rootFolder).IsFolderOrFile);
	}
	
	[TestMethod]
	public void RemoveTempAndParentStreamFolderHelper_1()
	{
		var appSettings = new AppSettings();

		var storage = new FakeIStorage(new List<string>{"/test", "/test/stream/"});
		new RemoveTempAndParentStreamFolderHelper(storage, appSettings).RemoveTempAndParentStreamFolder("/test/stream/");
		
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, storage.Info("/test/stream").IsFolderOrFile);
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, storage.Info("/test").IsFolderOrFile);
	}
	
	[TestMethod]
	public void RemoveTempAndParentStreamFolderHelper_1_List()
	{
		var appSettings = new AppSettings();

		var storage = new FakeIStorage(new List<string>{"/test", "/test/stream/"});
		new RemoveTempAndParentStreamFolderHelper(storage, appSettings).RemoveTempAndParentStreamFolder(new List<string>{"/test/stream/"});
		
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Deleted, storage.Info("/test/stream").IsFolderOrFile);
		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, storage.Info("/test").IsFolderOrFile);
	}
}
