using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.Helpers;
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
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new FileIndexItem("/sub/test8495.jpg")
		{
			Status = FileIndexItem.ExifStatus.Ok
		}, "/sub/test8495.jpg");

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Exists(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Exists(p => p.FilePath == "/sub");
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
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new List<FileIndexItem>{
			new FileIndexItem("/sub/test8495.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}}, true);// <- - - - add parent item is True

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Exists(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Exists(p => p.FilePath == "/sub");
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
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		var syncResult = await updateItem.NewItem(new List<FileIndexItem>{new FileIndexItem("/sub/test8495.jpg")
		{
			Status = FileIndexItem.ExifStatus.Ok
		}}, false); // <- - - - add parent item is FALSE

		Assert.IsNotNull(syncResult);
		var dbResult = await fakeQuery.GetAllRecursiveAsync();
		var itemItSelf =
			dbResult.Exists(p => p.FilePath == "/sub/test8495.jpg");
		var parentItem =
			dbResult.Exists(p => p.FilePath == "/sub");
		Assert.AreEqual(1, dbResult.Count); // 1
			
		Assert.IsTrue(itemItSelf);
		Assert.IsFalse(parentItem); // FALSE
	}

	[TestMethod]
	public async Task UpdateItem_IgnoreWhenOkAndSameStatus()
	{
		var item = new FileIndexItem("/test.jpg")
		{
			Status = FileIndexItem.ExifStatus.OkAndSame, 
			ColorClass = ColorClassParser.Color.None,
			Orientation = FileIndexItem.Rotation.Horizontal,
			ImageHeight = 2,
			ImageWidth = 3,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
		};
		
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>{item});
		var storage = new FakeIStorage(new List<string> { "/", "/sub" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		item.Tags = "updated";
		var result = await updateItem.UpdateItem(item, 1, "/test.jpg",true);
		
		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result?.Status);
	}
	
	[TestMethod]
	public async Task UpdateItem_AddParentItemAndUpdate()
	{
		var item = new FileIndexItem("/test.jpg")
		{
			Status = FileIndexItem.ExifStatus.OkAndSame, 
			ColorClass = ColorClassParser.Color.Extras, // different
			Orientation = FileIndexItem.Rotation.Horizontal,
			ImageHeight = 2,
			ImageWidth = 3,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
		};
		
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>{item});
		var storage = new FakeIStorage(new List<string> { "/", "/sub" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray() });
		
		var dbParentResultBefore = await fakeQuery.GetObjectByFilePathAsync("/");
		Assert.IsNull(dbParentResultBefore);
		
		var updateItem = new NewUpdateItemWrapper( fakeQuery, storage, new AppSettings(), null, new FakeIWebLogger());

		item.Tags = "updated";
		var result = await updateItem.UpdateItem(item, 1, "/test.jpg",true);
		
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);

		var dbParentResult = await fakeQuery.GetObjectByFilePathAsync("/");
		Assert.IsNotNull(dbParentResult);
	}
}
