using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public class DeleteItemTest
	{
		[TestMethod]
		public void Delete_FileNotFound_Ignore()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage());
			var deleteItem = new DeleteItem(new FakeIQuery(), new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/not-found", true);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, 	
				result.FirstOrDefault().Status);
		}

		[TestMethod]
		public void Delete_NotFoundOnDisk_Ignore()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage());
			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/exist-in-db.jpg")});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/exist-in-db.jpg", true);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, 	
				result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public void Delete_ReadOnly_Ignored()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"},
				new List<string>{"/readonly/test.jpg"}, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}));

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/readonly/test.jpg")});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings{ReadOnlyFolders = new List<string>{"/readonly"}}, selectorStorage);
			var result = deleteItem.Delete("/readonly/test.jpg", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, 	
				result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public void Delete_StatusNotDeleted_Ignored()
		{
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"}, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}));

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/test.jpg")});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/test.jpg", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, 	
				result.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public void Delete_IsFileRemoved()
		{
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.jpg"},
				new List<byte[]> {FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/test.jpg")
					{Tags = "!delete!"}});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/test.jpg", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 	
				result.FirstOrDefault().Status);
			
			Assert.IsNull(fakeQuery.GetObjectByFilePath("/test.jpg"));
			Assert.IsFalse(storage.ExistFile("/test.jpg"));
		}
		
		[TestMethod]
		public void Delete_IsJsonSideCarFileRemoved()
		{
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.jpg","/.starsky.test.jpg.json"},
				new List<byte[]> {FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/test.jpg")
					{Tags = "!delete!"}});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/test.jpg", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 	
				result.FirstOrDefault().Status);
			
			Assert.IsFalse(storage.ExistFile("/.starsky.test.jpg.json"));
		}
		
		[TestMethod]
		public void Delete_IsXmpSideCarFileRemoved()
		{
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.dng","/test.xmp"},
				new List<byte[]> {FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/test.dng")
					{Tags = "!delete!"}});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/test.dng", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 	
				result.FirstOrDefault().Status);
			
			Assert.IsFalse(storage.ExistFile("/test.xmp"));
		}
		
		[TestMethod]
		public void Delete_IsFolderRemoved()
		{
			var storage = new FakeIStorage(new List<string> {"/test","/"},
				new List<string> (),
				new List<byte[]> {FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);

			var fakeQuery =
				new FakeIQuery(new List<FileIndexItem> {new FileIndexItem("/test")
					{IsDirectory = true, Tags = "!delete!"}});
			var deleteItem = new DeleteItem( fakeQuery,new AppSettings(), selectorStorage);
			var result = deleteItem.Delete("/test", true);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 	
				result.FirstOrDefault().Status);
			
			Assert.IsNull(fakeQuery.GetObjectByFilePath("/test"));
			Assert.IsFalse(storage.ExistFolder("/test"));
		}
	}
}
