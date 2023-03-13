using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public class NewUpdateItemWrapperTest
{
	[TestMethod]
	public async Task NewItem_Single_WrongStatus()
	{
		var fakeQuery = new FakeIQuery();
		var updateItem = new NewUpdateItemWrapper( fakeQuery, new FakeIStorage(), new AppSettings(), null, new FakeIWebLogger());
		
		var syncResult = await updateItem.NewItem(new FileIndexItem(), "/sub/test8495.jpg");

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		Assert.AreEqual(0, dbResult.Count);
	}
		
	[TestMethod]
	public async Task NewItem_Single_Ok_AddParentItem()
	{
		var fakeQuery = new FakeIQuery();
		var storage = new FakeIStorage(new List<string> { "/", "/sub" },
			new List<string> { "/sub/test8495.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new FileIndexItem("/sub/test8495.jpg")
		{
			Status = FileIndexItem.ExifStatus.Ok
		}, "/sub/test8495.jpg");

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Any(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Any(p => p.FilePath == "/sub");
		Assert.AreEqual(2, dbResult.Count);
			
		Assert.IsTrue(itemItSelf);
		Assert.IsTrue(parentItem);
	}
		
	[TestMethod]
	public async Task NewItem_List_Ok_AddParentItem()
	{
		var fakeQuery = new FakeIQuery();
		var storage = new FakeIStorage(new List<string> { "/", "/sub" },
			new List<string> { "/sub/test8495.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new List<FileIndexItem>{
			new FileIndexItem("/sub/test8495.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}, true);// <- - - - add parent item is True

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Any(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Any(p => p.FilePath == "/sub");
		Assert.AreEqual(2, dbResult.Count);
			
		Assert.IsTrue(itemItSelf);
		Assert.IsTrue(parentItem);
	}
		
	[TestMethod]
	public async Task NewItem_List_Ok_Ignore_AddParentItem()
	{
		var fakeQuery = new FakeIQuery();
		var storage = new FakeIStorage(new List<string> { "/", "/sub" },
			new List<string> { "/sub/test8495.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new List<FileIndexItem>{new FileIndexItem("/sub/test8495.jpg")
		{
			Status = FileIndexItem.ExifStatus.Ok
		}}, false); // <- - - - add parent item is FALSE

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Any(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Any(p => p.FilePath == "/sub");
		Assert.AreEqual(1, dbResult.Count); // 1
			
		Assert.IsTrue(itemItSelf);
		Assert.IsFalse(parentItem); // FALSE
	}
}
