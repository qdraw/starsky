using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.Helpers;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public class DeleteStatusHelperTest
{
	[TestMethod]
	public void AddDeleteStatus_Null()
	{
		var result = DeleteStatusHelper.AddDeleteStatus(null as FileIndexItem);
		Assert.IsNull(result);
	}
		
	[TestMethod]
	public void AddDeleteStatus_NotDeleted()
	{
		var item = new FileIndexItem() {Tags = "test", Status = FileIndexItem.ExifStatus.Ok};

		var result = DeleteStatusHelper.AddDeleteStatus(item);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result.Status);
	}
		
	[TestMethod]
	public void AddDeleteStatus_Deleted()
	{
		var item = new FileIndexItem() {Tags = TrashKeyword.TrashKeywordString};

		var result = DeleteStatusHelper.AddDeleteStatus(item);
		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,result.Status);
	}

	[TestMethod]
	public void List()
	{
		var item = new FileIndexItem() {Tags = TrashKeyword.TrashKeywordString};

		var result = DeleteStatusHelper.AddDeleteStatus(new List<FileIndexItem>{item});
		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted,result[0].Status);
	}
	
	[TestMethod]
	public void List_Same()
	{
		var item = new FileIndexItem() {Tags = TrashKeyword.TrashKeywordString};

		var result = DeleteStatusHelper.AddDeleteStatus(new List<FileIndexItem>{item}, FileIndexItem.ExifStatus.DeletedAndSame);
		Assert.AreEqual(FileIndexItem.ExifStatus.DeletedAndSame,result[0].Status);
	}
}
