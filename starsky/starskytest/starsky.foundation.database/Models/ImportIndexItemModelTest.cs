using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public sealed class ImportIndexItemModelTest
{
	[TestMethod]
	public void Constructor_Default_HasNoStructureOverride()
	{
		var item = new ImportIndexItem();

		Assert.IsNotNull(item.Structure);
	}

	[TestMethod]
	public void Constructor_WithAppSettings_CopiesStructure()
	{
		var appSettings = new AppSettings
		{
			Structure = new AppSettingsStructureModel("/yyyy/MM/{filenamebase}.ext")
		};

		var item = new ImportIndexItem(appSettings);

		Assert.AreEqual(appSettings.Structure, item.Structure);
	}

	[TestMethod]
	public void DefaultValues_AreSetCorrectly()
	{
		var item = new ImportIndexItem();

		Assert.AreEqual(string.Empty, item.FileHash);
		Assert.AreEqual(string.Empty, item.FilePath);
		Assert.AreEqual(string.Empty, item.MakeModel);
		Assert.AreEqual(string.Empty, item.Artist);
		Assert.AreEqual(string.Empty, item.SourceFullFilePath);
		Assert.AreEqual(string.Empty, item.Origin);
		Assert.AreEqual(ImportStatus.Default, item.Status);
		Assert.IsFalse(item.DateTimeFromFileName);
		Assert.AreEqual(0, item.Size);
	}

	[TestMethod]
	public void GetFileHashWithUpdate_NoFileIndexItem_ReturnsFileHash()
	{
		var item = new ImportIndexItem { FileHash = "abc123", FileIndexItem = null };

		var result = item.GetFileHashWithUpdate();

		Assert.AreEqual("abc123", result);
	}

	[TestMethod]
	public void GetFileHashWithUpdate_NoFileIndexItemAndNoFileHash_ReturnsEmptyString()
	{
		var item = new ImportIndexItem { FileHash = null, FileIndexItem = null };

		var result = item.GetFileHashWithUpdate();

		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void GetFileHashWithUpdate_WithFileIndexItem_ReturnsFileIndexItemFileHash()
	{
		var item = new ImportIndexItem
		{
			FileHash = "old-hash",
			FileIndexItem = new FileIndexItem { FileHash = "new-hash" }
		};

		var result = item.GetFileHashWithUpdate();

		Assert.AreEqual("new-hash", result);
	}

	[TestMethod]
	public void GetFileHashWithUpdate_WithFileIndexItemNullFileHash_ReturnsEmptyString()
	{
		var item = new ImportIndexItem
		{
			FileHash = "old-hash", FileIndexItem = new FileIndexItem { FileHash = null }
		};

		var result = item.GetFileHashWithUpdate();

		Assert.AreEqual(string.Empty, result);
	}
}
