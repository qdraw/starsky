﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			Assert.AreEqual(true,statusBool);
		}

	}
}
