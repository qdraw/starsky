using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SyncSingleFileTest
	{
		private readonly IStorage _iStorageFake;

		public SyncSingleFileTest()
		{
			_iStorageFake = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg","/color_class_test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes, 
					FakeCreateAn.CreateAnImageColorClass.Bytes});
		}

		[TestMethod]
		public async Task FileType_NotSupported()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			var result = await sync.SingleFile("/non_exist.ext");

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result.Status);
		}
		
		[TestMethod]
		public async Task ImageFormat_Corrupt()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			
			var storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/corrupt.jpg"},
				new List<byte[]>{new byte[5]});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(storage), new ConsoleWrapper());
			var result = await sync.SingleFile("/corrupt.jpg");

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result.Status);
		}
		
		[TestMethod]
		public async Task AddNewFile()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/test.jpg");

			// should add files to db
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(fileIndexItem.FilePath, "/test.jpg");
			
			// should not duplicate add items
			var count= fakeQuery.GetAllFiles("/").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
		}
		
		[TestMethod]
		public async Task AddNewFile_WithParentFolders()
		{
			var iStorageFake = new FakeIStorage(new List<string>{"/level/deep/"},
				new List<string>{"/level/deep/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes});
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/level/deep/test.jpg");

			var detailView = fakeQuery.SingleItem("/level/deep/test.jpg");
			// should add files to db
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(fileIndexItem.FilePath, "/level/deep/test.jpg");
			
			// should not duplicate add items
			var count= fakeQuery.GetAllFiles("/level/deep").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			// should add parent items
				// root folder
			var rootFolder = fakeQuery.SingleItem("/")?.FileIndexItem;
			Assert.IsNotNull(rootFolder);
			Assert.AreEqual(true,rootFolder.IsDirectory);

			// sub folder 'level folder
			var levelFolder = fakeQuery.SingleItem("/level")?.FileIndexItem;
			Assert.IsNotNull(levelFolder);
			Assert.AreEqual(true,levelFolder.IsDirectory);

			// sub folder 'level deep folder
			var levelDeepFolder = fakeQuery.SingleItem("/level/deep")?.FileIndexItem;
			Assert.IsNotNull(levelDeepFolder);
			Assert.AreEqual(true,levelDeepFolder.IsDirectory);
		}

		[TestMethod]
		public async Task FileAlreadyExist_WithSameFileHash()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/test.jpg");

			var count= fakeQuery.GetAllFiles("/").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(fileIndexItem.FilePath, "/test.jpg");
		}

		[TestMethod]
		public async Task FileAlreadyExist_With_Changed_ByteSize()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash,
					Size = 82153441 // < wrong byte size
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/test.jpg");
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg").FileIndexItem;
			// checks if the byte size is updated
			Assert.AreEqual(_iStorageFake.Info("/test.jpg").Size, fileIndexItem.Size);
		}
		
		[TestMethod]
		public async Task FileAlreadyExist_With_Same_ByteSize()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");

			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = fileHash,
					Size = _iStorageFake.Info("/test.jpg").Size, // < right byte size
					Tags = "the tags should not be updated" // <= the tags in /test.jpg is nothing
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/test.jpg");
			
			var fileIndexItem = fakeQuery.SingleItem("/test.jpg").FileIndexItem;

			Assert.AreNotEqual(string.Empty, fileIndexItem.Tags);
			Assert.AreEqual("the tags should not be updated", fileIndexItem.Tags);
		}

		[TestMethod]
		public async Task FileAlreadyExist_With_Changed_FileHash()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = "THIS_IS_THE_OLD_HASH",
					Size = 8951862 // byte size is different
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			await sync.SingleFile("/test.jpg");

			var count= fakeQuery.GetAllFiles("/").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg",fileIndexItem.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem.FileHash);
		}
		
		[TestMethod]
		public async Task FileAlreadyExist_With_Changed_FileHash_MetaDataCheck()
		{
			var (fileHash, _) = await new FileHash(_iStorageFake)
				.GetHashCodeAsync("/color_class_test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/color_class_test.jpg")
				{
					FileHash = "THIS_IS_THE_OLD_HASH"
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake), new ConsoleWrapper());
			
			await sync.SingleFile("/color_class_test.jpg");
			
			var fileIndexItem = fakeQuery.SingleItem("/color_class_test.jpg").FileIndexItem;
			
			Assert.IsNotNull(fileIndexItem);
			Assert.AreEqual(fileHash, fileIndexItem.FileHash);
			Assert.AreEqual("Magland", fileIndexItem.LocationCity);
			Assert.AreEqual(9, fileIndexItem.Aperture);
			Assert.AreEqual(400, fileIndexItem.IsoSpeed);
			Assert.AreEqual("tete de balacha, bergtop, mist, flaine", fileIndexItem.Tags);
			Assert.AreEqual(ColorClassParser.Color.Winner, fileIndexItem.ColorClass);
		}
	}
}
