using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

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
		var item = new ThumbnailItem("test");
		Assert.AreEqual("test", item.FileHash);
	}
	
		
	[TestMethod]
	public void ThumbnailItemCtor3()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Large);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(null, item.ExtraLarge);
	}
			
	[TestMethod]
	public void ThumbnailItemCtor4()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Large, true);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.Large);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor5()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.ExtraLarge, true);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.ExtraLarge);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor6()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.TinyMeta, true);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.TinyMeta);
	}
	
			
	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ThumbnailItemCtor7()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Unknown, true);
		Assert.AreEqual("test", item.FileHash);
	}
	
				
	[TestMethod]
	public void ThumbnailItemCtor8()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Small, true);
		Assert.AreEqual("test", item.FileHash);
		Assert.AreEqual(true, item.Small);
	}
	
	[TestMethod]
	public void ThumbnailItem_reasons()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Small, true){Reasons = "test"};
		Assert.AreEqual("test", item.Reasons);
	}
}
