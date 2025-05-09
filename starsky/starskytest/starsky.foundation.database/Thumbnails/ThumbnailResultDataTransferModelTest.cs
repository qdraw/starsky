using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Thumbnails;

namespace starskytest.starsky.foundation.database.Thumbnails;

[TestClass]
public class ThumbnailResultDataTransferModelTest
{
	[TestMethod]
	public void ThumbnailResultDataTransferModel_Ctor_1()
	{
		var model = new ThumbnailResultDataTransferModel("test");
		Assert.AreEqual("test", model.FileHash);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Ctor_2()
	{
		var model = new ThumbnailResultDataTransferModel("test", true);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Ctor_3()
	{
		var model = new ThumbnailResultDataTransferModel("test", true, true);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Ctor_4()
	{
		var model = new ThumbnailResultDataTransferModel("test",
			true, true, true);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsTrue(model.Large);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Ctor_5()
	{
		var model = new ThumbnailResultDataTransferModel("test", true, true, true, true);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsTrue(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_TinyMeta()
	{
		var model = new ThumbnailResultDataTransferModel("test", true, true, true, true);
		model.Change(ThumbnailSize.TinyMeta, false);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsFalse(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsTrue(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_TinyIcon()
	{
		var model = new ThumbnailResultDataTransferModel("test",
			true, true, true, true);
		Assert.IsNull(model.TinyIcon);
		model.Change(ThumbnailSize.TinyIcon, false);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsFalse(model.TinyIcon);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsTrue(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_TinyIcon_Status()
	{
		var model = new ThumbnailResultDataTransferModel("test",
			true, true, true, true);
		model.Change(ThumbnailSize.TinyIcon, true);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyIcon);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsTrue(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_Small()
	{
		var model = new ThumbnailResultDataTransferModel("test", true, true, true, true);
		model.Change(ThumbnailSize.Small, false);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsFalse(model.Small);
		Assert.IsTrue(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_Large()
	{
		var model = new ThumbnailResultDataTransferModel("test", true,
			true, true, true);
		model.Change(ThumbnailSize.Large, false);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsFalse(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_Large_Null()
	{
		var model = new ThumbnailResultDataTransferModel("test", true, true, true, true);
		model.Change(ThumbnailSize.Large);
		Assert.AreEqual("test", model.FileHash);
		Assert.IsTrue(model.TinyMeta);
		Assert.IsTrue(model.Small);
		Assert.IsNull(model.Large);
		Assert.IsTrue(model.ExtraLarge);
	}

	[TestMethod]
	public void ThumbnailResultDataTransferModel_Change_OutOfRange()
	{
		var model = new ThumbnailResultDataTransferModel("test",
			true, true, true, true);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
			model.Change(ThumbnailSize.Unknown, false));
	}
}
