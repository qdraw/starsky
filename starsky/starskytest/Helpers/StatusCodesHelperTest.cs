using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.query.Models;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public class StatusCodesHelperTest
	{
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_NotFoundNotInIndex()
		{
			var appSettings = new AppSettings();
			var status = new StatusCodesHelper(appSettings,new StorageSubPathFilesystem(appSettings)).FileCollectionsCheck(null);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_DirReadOnly()
		{
			// this is the only diff -->>
			var appSettings = new AppSettings{ReadOnlyFolders = new List<string>{"/"}};
			var detailView = new DetailView
			{
				IsDirectory = true,
				SubPath = "/"
			};
			var status = new StatusCodesHelper(appSettings,new StorageSubPathFilesystem(appSettings)).FileCollectionsCheck(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.DirReadOnly,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_InjectFakeIStorage_GoodSituation()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = false,
				SubPath = "/test.jpg",
				FileIndexItem = new FileIndexItem{ParentDirectory = "/", FileName = "test.jpg", CollectionPaths = new List<string>{"/test.jpg"}}
			};
			var istorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var status = new StatusCodesHelper(appSettings,istorage).FileCollectionsCheck(detailView);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,status);
		}
		
		
		[TestMethod]
		public void StatusCodesHelperTest_InjectFakeIStorage_FileDeletedTag()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = false,
				SubPath = "/test.jpg",
				FileIndexItem = new FileIndexItem{ParentDirectory = "/", Tags = "!delete!", FileName = "test.jpg", CollectionPaths = new List<string>{"/test.jpg"}}
			};
			var istorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var status = new StatusCodesHelper(appSettings,istorage).FileCollectionsCheck(detailView);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_InjectFakeIStorage_NotExitSituation()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = false,
				SubPath = "/404.jpg",
				FileIndexItem = new FileIndexItem{ParentDirectory = "/", FileName = "404.jpg", CollectionPaths = new List<string>{"/404.jpg"}}
			};
			var istorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var status = new StatusCodesHelper(appSettings,istorage).FileCollectionsCheck(detailView);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_FileCollectionsCheck_NotFoundIsDir()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = true,
				SubPath = "/"
			};
			var status = new StatusCodesHelper(appSettings,new StorageSubPathFilesystem(appSettings)).FileCollectionsCheck(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundIsDir,status);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundIsDir()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundIsDir;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper(appSettings,new StorageSubPathFilesystem(appSettings)).ReturnExifStatusError(statusModel, statusResults,
					fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_DirReadOnly()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.DirReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundNotInIndex()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundNotInIndex;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_NotFoundSourceMissing()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.NotFoundSourceMissing;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
	
		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_ReadOnly()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(false,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyDenied_true()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReadonlyDenied(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyDenied_false()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.Ok;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = new StatusCodesHelper().ReadonlyDenied(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(false,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyAllowed_true()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			new StatusCodesHelper().ReadonlyAllowed(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly,fileIndexResultsList.FirstOrDefault().Status);
		}
		
		[TestMethod]
		[ExpectedException(typeof(DllNotFoundException))]
		public void StatusCodesHelperTest_WrongInput()
		{
			new StatusCodesHelper(null,null).FileCollectionsCheck(null);
		}
		
		[TestMethod]
		[ExpectedException(typeof(DllNotFoundException))]
		public void StatusCodesHelperTest_WrongInput2()
		{
			new StatusCodesHelper(new AppSettings(), null).FileCollectionsCheck(null);
		}

	}
}
