using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Helpers;
using starsky.feature.import.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.import.Helpers
{
	[TestClass]
	public sealed class UpdateImportTransformationsTest
	{
		[TestMethod]
		public async Task UpdateTransformations_ShouldUpdate_ColorClass_IndexModeOn()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test.xmp"},
				new List<byte[]>{CreateAnPng.Bytes.ToArray(),CreateAnXmp.Bytes.ToArray()});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), 
				new FakeSelectorStorage(storage), appSettings, 
				new FakeIThumbnailQuery());
			
			var query = new FakeIQuery();
			await query.AddItemAsync(new FileIndexItem("/test.jpg"){FileHash = "test"});

			UpdateImportTransformations.QueryUpdateDelegate updateItemAsync = query.UpdateItemAsync;
			
			await updateImportTransformations.UpdateTransformations(updateItemAsync,
				new FileIndexItem("/test.jpg"){ColorClass = ColorClassParser.Color.Typical}, 0,
				false, true);

			var updatedItem = await query.GetObjectByFilePathAsync("/test.jpg");
			Assert.AreEqual(ColorClassParser.Color.Typical,updatedItem.ColorClass);
		}
		
		[TestMethod]
		public async Task UpdateTransformations_ShouldNotUpdate_IndexOff()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test.xmp"},
				new List<byte[]>{CreateAnPng.Bytes.ToArray(),CreateAnXmp.Bytes.ToArray()});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), 
				new FakeSelectorStorage(storage), appSettings, 
				new FakeIThumbnailQuery());
			
			var query = new FakeIQuery();
			await query.AddItemAsync(new FileIndexItem("/test.jpg"){FileHash = "test"});

			UpdateImportTransformations.QueryUpdateDelegate updateItemAsync = query.UpdateItemAsync;
			
			await updateImportTransformations.UpdateTransformations(updateItemAsync,
				new FileIndexItem("/test.jpg"){ColorClass = ColorClassParser.Color.Typical}, 0,
				false, false);

			var updatedItem = await query.GetObjectByFilePathAsync("/test.jpg");
			// Are NOT equal!
			Assert.AreNotEqual(ColorClassParser.Color.Typical,updatedItem.ColorClass);
		}

		[TestMethod]
		public async Task UpdateTransformations_ShouldUpdate_Description_IndexModeOn()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test.xmp"},
				new List<byte[]>{CreateAnPng.Bytes.ToArray(),CreateAnXmp.Bytes.ToArray()});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), new FakeSelectorStorage(storage), appSettings, 
				new FakeIThumbnailQuery());
			
			var query = new FakeIQuery();
			await query.AddItemAsync(new FileIndexItem("/test.jpg"){FileHash = "test"});

			UpdateImportTransformations.QueryUpdateDelegate updateItemAsync = query.UpdateItemAsync;
			
			await updateImportTransformations.UpdateTransformations(updateItemAsync,
				new FileIndexItem("/test.jpg"){Description = "test-ung"}, -1,
				true, true);

			var updatedItem = await query.GetObjectByFilePathAsync("/test.jpg");
			Assert.AreEqual("test-ung",updatedItem.Description);
		}

		[TestMethod]
		public void DateTimeParsedComparedNamesList_Contain()
		{
			var list = UpdateImportTransformations.DateTimeParsedComparedNamesList();
			Assert.AreEqual(2,list.Count);
			Assert.AreEqual(nameof(FileIndexItem.Description).ToLowerInvariant(),list[0]);
			Assert.AreEqual(nameof(FileIndexItem.DateTime).ToLowerInvariant(),list[1]);
		}
		
		
		[TestMethod]
		public void ColorClassComparedNamesList_Contain()
		{
			var list = UpdateImportTransformations.ColorClassComparedNamesList(new List<string>());
			Assert.AreEqual(1,list.Count);
			Assert.AreEqual(nameof(FileIndexItem.ColorClass).ToLowerInvariant(),list[0]);
		}
	}
	
}

