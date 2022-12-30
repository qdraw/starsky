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
		Assert.AreEqual(item.FileHash, "test");
	}
	
		
	[TestMethod]
	public void ThumbnailItemCtor3()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Large);
		Assert.AreEqual(item.FileHash, "test");
		Assert.AreEqual(item.ExtraLarge, null);
	}
			
	[TestMethod]
	public void ThumbnailItemCtor4()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Large, true);
		Assert.AreEqual(item.FileHash, "test");
		Assert.AreEqual(item.Large, true);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor5()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.ExtraLarge, true);
		Assert.AreEqual(item.FileHash, "test");
		Assert.AreEqual(item.ExtraLarge, true);
	}
	
	[TestMethod]
	public void ThumbnailItemCtor6()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.TinyMeta, true);
		Assert.AreEqual(item.FileHash, "test");
		Assert.AreEqual(item.TinyMeta, true);
	}
	
			
	[TestMethod]
	[ExpectedException(typeof(ArgumentOutOfRangeException))]
	public void ThumbnailItemCtor7()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Unknown, true);
		Assert.AreEqual(item.FileHash, "test");
	}
	
				
	[TestMethod]
	public void ThumbnailItemCtor8()
	{
		var item = new ThumbnailItem("test", ThumbnailSize.Small, true);
		Assert.AreEqual(item.FileHash, "test");
		Assert.AreEqual(item.Small, true);
	}
}
