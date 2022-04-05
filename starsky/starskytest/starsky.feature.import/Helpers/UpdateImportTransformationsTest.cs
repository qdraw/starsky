using System;
using System.Collections.Generic;
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
	public class UpdateImportTransformationsTest
	{
		[TestMethod]
		public async Task UpdateTransformations_ShouldUpdate_ColorClass_IndexModeOn()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.jpg","/test.xmp"},
				new List<byte[]>{CreateAnPng.Bytes,CreateAnXmp.Bytes});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), new FakeSelectorStorage(storage), appSettings);
			
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
				new List<byte[]>{CreateAnPng.Bytes,CreateAnXmp.Bytes});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), new FakeSelectorStorage(storage), appSettings);
			
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
				new List<byte[]>{CreateAnPng.Bytes,CreateAnXmp.Bytes});
			var appSettings = new AppSettings();
			
			var updateImportTransformations = new UpdateImportTransformations(new FakeIWebLogger(),
				new FakeExifTool(storage, appSettings), new FakeSelectorStorage(storage), appSettings);
			
			var query = new FakeIQuery();
			await query.AddItemAsync(new FileIndexItem("/test.jpg"){FileHash = "test"});

			UpdateImportTransformations.QueryUpdateDelegate updateItemAsync = query.UpdateItemAsync;
			
			await updateImportTransformations.UpdateTransformations(updateItemAsync,
				new FileIndexItem("/test.jpg"){Description = "test-ung"}, -1,
				true, true);

			var updatedItem = await query.GetObjectByFilePathAsync("/test.jpg");
			Assert.AreEqual("test-ung",updatedItem.Description);
		}
	}
	
}

