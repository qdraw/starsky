using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public class ThumbnailItemTest
{
	[TestMethod]
	public void ThumbnailItemCtor()
	{
		var item = new ThumbnailItem();
		Assert.AreEqual(item.FileHash, string.Empty);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor2()
	{
		var item = new ThumbnailItem("test",null,null,null,null);
		Assert.AreEqual("test", item.FileHash);
	}
	
		
	[TestMethod]
	public void ThumbnailItemCtor3()
	{
		var item = new ThumbnailItem("test",null,null,true,null);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(null, item.ExtraLarge);
	}
			
	[TestMethod]
	public void ThumbnailItemCtor4()
	{
		var item = new ThumbnailItem("test",null,null,true,null);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.Large);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor5()
	{
		var item = new ThumbnailItem("test",null,null,null,true);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.ExtraLarge);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor6()
	{
		var item = new ThumbnailItem("test",true,null,true,null);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.TinyMeta);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor8()
	{
		var item = new ThumbnailItem("test",null,true,true,null);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.Small);
	}
	
	[TestMethod]
	public void ThumbnailItem_reasons()
	{
		var item = new ThumbnailItem("test",null,null,null,null)
		{
			Reasons = "test"
		};
		Assert.AreEqual("test", item.Reasons);
	}
}
