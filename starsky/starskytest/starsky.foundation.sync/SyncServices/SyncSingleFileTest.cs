using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
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
				new List<string>{"/test.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes});
		}
		
		[TestMethod]
		public async Task AddNewFile()
		{
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>());
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake));
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
				new FakeSelectorStorage(iStorageFake));
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
				new FakeSelectorStorage(_iStorageFake));
			await sync.SingleFile("/test.jpg");

			var count= fakeQuery.GetAllFiles("/").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual(fileIndexItem.FilePath, "/test.jpg");
		}

		[TestMethod]
		public async Task FileAlreadyExist_With_Changed_FileHash()
		{
			
			var (fileHash, _) = await new FileHash(_iStorageFake).GetHashCodeAsync("/test.jpg");
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = "SOME_OTHER_HASH_THAT_IS_CHANGED"
				}
			});
			
			var sync = new SyncSingleFile(new AppSettings(), fakeQuery,
				new FakeSelectorStorage(_iStorageFake));
			await sync.SingleFile("/test.jpg");

			var count= fakeQuery.GetAllFiles("/").Count(p => p.FileName == "test.jpg");
			Assert.AreEqual(1,count);
			
			var detailView = fakeQuery.SingleItem("/test.jpg");
			
			Assert.IsNotNull(detailView);
			var fileIndexItem = detailView.FileIndexItem;
			Assert.AreEqual("/test.jpg",fileIndexItem.FilePath);
			Assert.AreEqual(fileHash, fileIndexItem.FileHash);
		}
	}
}
