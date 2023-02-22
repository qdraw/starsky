﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public sealed class StatusCodesHelperTest
	{
		[TestMethod] 
		public void IsDeletedStatus_Null1_Default()
		{
			DetailView detailView = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var status = StatusCodesHelper.IsDeletedStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod] 
		public void IsDeletedStatus_Null2_Default()
		{
			var detailView = new DetailView();
			// ReSharper disable once ExpressionIsAlwaysNull
			var status = StatusCodesHelper.IsDeletedStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
				
		[TestMethod] 
		public void IsDeletedStatus_Null3_Default()
		{
			
			var overWriteFileIndexItem = new FileIndexItem();
			var propertyObject = overWriteFileIndexItem.GetType().GetProperty(nameof(FileIndexItem.Tags));
			propertyObject?.SetValue(overWriteFileIndexItem, null, null); // <-- this could not happen
			// Getter returns string.Empty
			
			var detailView = new DetailView
			{
				FileIndexItem = overWriteFileIndexItem
			};
			// ReSharper disable once ExpressionIsAlwaysNull
			var status = StatusCodesHelper.IsDeletedStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod] 
		public void IsDeletedStatus_TagsNull_Default1()
		{
			var detailView = new DetailView{ FileIndexItem = new FileIndexItem
			{
				Tags = null!
			}};
			
			var status = StatusCodesHelper.IsDeletedStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod]
		public void IsReadOnlyStatus_ParentDirectory_Null_Default()
		{
			var detailView = new DetailView{ FileIndexItem = new FileIndexItem
			{
				ParentDirectory = null!
			}};
			var appSettings = new AppSettings();
			var status = new StatusCodesHelper(appSettings).IsReadOnlyStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod]
		public void IsReadOnlyStatus_Null_Default()
		{
			DetailView detailView = null;
			var appSettings = new AppSettings();
			// ReSharper disable once ExpressionIsAlwaysNull
			var status = new StatusCodesHelper(appSettings).IsReadOnlyStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod]
		public void IsReadOnlyStatus_DetailView_DirReadOnly()
		{
			// this is the only diff -->>
			var appSettings = new AppSettings{ReadOnlyFolders = new List<string>{"/"}};
			var detailView = new DetailView
			{
				IsDirectory = true,
				SubPath = "/"
			};
			var status = new StatusCodesHelper(appSettings).IsReadOnlyStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.DirReadOnly,status);
		}
		
		[TestMethod]
		[ExpectedException(typeof(DllNotFoundException))]
		public void IsReadOnlyStatus_DetailView_AppSettingsNull()
		{
			DetailView detailView = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			new StatusCodesHelper(null).IsReadOnlyStatus(detailView);
			// expect DllNotFoundException
		}
				
		[TestMethod]
		public void IsReadOnlyStatus_DetailView_Null()
		{
			DetailView detailView = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var status = new StatusCodesHelper(new AppSettings()).IsReadOnlyStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.Default,status);
		}
		
		[TestMethod]
		public void IsReadOnlyStatus_FileIndexItem_DirReadOnly()
		{
			// this is the only diff -->>
			var appSettings = new AppSettings{ReadOnlyFolders = new List<string>{"/"}};
			var detailView = new FileIndexItem
			{
				IsDirectory = true,
				FilePath = "/"
			};
			var status = new StatusCodesHelper(appSettings).IsReadOnlyStatus(detailView);
			Assert.AreEqual(FileIndexItem.ExifStatus.DirReadOnly,status);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_InjectFakeIStorage_FileDeletedTag()
		{
			var appSettings = new AppSettings();
			var detailView = new DetailView
			{
				IsDirectory = false,
				SubPath = "/test.jpg",
				FileIndexItem = new FileIndexItem{ParentDirectory = "/", 
					Tags = TrashKeyword.TrashKeywordString, FileName = "test.jpg", CollectionPaths = new List<string>{"/test.jpg"}}
			};
			var istorage = new FakeIStorage(new List<string> {"/"}, 
				new List<string> {"/test.jpg"});
			var status = StatusCodesHelper.IsDeletedStatus(detailView);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,status);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_DirReadOnly()
		{
			var appSettings = new AppSettings();
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.DirReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = StatusCodesHelper.ReturnExifStatusError(statusModel, statusResults,
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
			var statusBool = StatusCodesHelper.ReturnExifStatusError(statusModel, statusResults,
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
			var statusBool = StatusCodesHelper.ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
	
		[TestMethod]
		public void StatusCodesHelperTest_ReturnExifStatusError_ReadOnly()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = StatusCodesHelper.ReturnExifStatusError(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}

		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyDenied_true()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = StatusCodesHelper.ReadonlyDenied(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(true,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyDenied_false()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.Ok;
			var fileIndexResultsList = new List<FileIndexItem>();
			var statusBool = StatusCodesHelper.ReadonlyDenied(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(false,statusBool);
		}
		
		[TestMethod]
		public void StatusCodesHelperTest_ReadonlyAllowed_true()
		{
			var statusModel = new FileIndexItem();
			var statusResults = FileIndexItem.ExifStatus.ReadOnly;
			var fileIndexResultsList = new List<FileIndexItem>();
			StatusCodesHelper.ReadonlyAllowed(statusModel, statusResults,
				fileIndexResultsList);
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly,fileIndexResultsList.FirstOrDefault().Status);
		}
	}
}
