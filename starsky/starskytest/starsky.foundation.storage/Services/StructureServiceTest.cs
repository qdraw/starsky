using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public sealed class StructureServiceTest
{
	[TestMethod]
	public void ParseFileName_DefaultDate()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
		var importItem = new StructureService(new FakeIStorage(), structure);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
		var fileName = importItem.ParseFileName(dateTime,
			string.Empty, "jpg");
		Assert.AreEqual("00010101_000000.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_LotsOfEscapeChars()
	{
		const string structure = "/yyyyMMdd_HHmmss_\\\\\\h\\\\\\m.ext";
		var importItem = new StructureService(new FakeIStorage(), structure);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
		var fileName = importItem.ParseFileName(dateTime,
			string.Empty, "jpg");
		Assert.AreEqual("00010101_000000_hm.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_FileNameWithAppendix()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_\\d.ext";
		var importItem = new StructureService(new FakeIStorage(), structure);
		var dateTime = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Local);
		var fileName = importItem.ParseFileName(dateTime,
			string.Empty, "jpg");
		Assert.AreEqual("00010101_000000_d.jpg", fileName);
	}

	[TestMethod]
	public void ParseFileName_FileNameBase()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var importItem = new StructureService(new FakeIStorage(), structure);
		var fileName = importItem.ParseFileName(
			new DateTime(2020, 01, 01, 01, 01, 01, DateTimeKind.Local),
			"test", "jpg");

		Assert.AreEqual("20200101_010101_test.jpg", fileName);
	}

	[TestMethod]
	[ExpectedException(typeof(FieldAccessException))]
	public void ParseFileName_FieldAccessException_Null()
	{
		new StructureService(new FakeIStorage(), null!).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));
		// ExpectedException
	}

	[TestMethod]
	public void ParseSubfolders_TestFolder_RealFs()
	{
		const string structure = "/\\te\\s*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var storage = new StorageSubPathFilesystem(
			new AppSettings { StorageFolder = new CreateAnImage().BasePath }, new FakeIWebLogger());
		storage.CreateDirectory("test");

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_Asterisk_Test_Folder()
	{
		const string structure = "/\\t*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var storage = new FakeIStorage(
			["/", "/test", "/something"],
			[]);

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_ReturnNewChildFolder()
	{
		var storage = new FakeIStorage(
			["/2020", "/2020/01"],
			[]);

		var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/2020/01/2020_01_01", result);
	}

	[TestMethod]
	public void ParseSubfolders_DefaultAsterisk()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string>());

		const string structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/default", result);
	}

	[TestMethod]
	public void ParseSubfolders_ExistingAsterisk()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/any" },
			new List<string>());

		const string structure = "/*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/any", result);
	}

	[TestMethod]
	public void ParseSubfolders_GetExistingFolder()
	{
		var storage = new FakeIStorage(
			new List<string>
			{
				"/",
				"/2020",
				"/2020/01",
				"/2020/01/2020_01_01 test",
				"/2020/01/2020_01_01 test/ignore"
			},
			new List<string>());

		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/2020/01/2020_01_01 test", result);
	}

	[TestMethod]
	public void ParseSubfolders_GetExistingPreferSimpleName()
	{
		var storage = new FakeIStorage(
			new List<string>
			{
				"/",
				"/2020",
				"/2020/01",
				"/2020/01/2020_01_01",
				"/2020/01/2020_01_01 test"
			},
			new List<string>());

		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local));

		Assert.AreEqual("/2020/01/2020_01_01", result);
	}

	[TestMethod]
	public void ParseSubfolders_FileNameBaseOnFolder()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>());

		const string structure = "/{filenamebase}/file.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local), "test");
		Assert.AreEqual("/test", result);
	}

	[TestMethod]
	public void ParseSubfolders_ExtensionWantedInFolderName()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>());

		const string structure = "/con\\ten\\t.ext/file.ext";

		var result = new StructureService(storage, structure).ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local), "test");
		Assert.AreEqual("/content.unknown", result);
	}

	[TestMethod]
	public void ParseSubfolders_FieldAccessException_String()
	{
		const string structure = "";
		var sut = new StructureService(new FakeIStorage(), structure);
		Assert.ThrowsException<FieldAccessException>(() => sut.ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local)));
	}

	[TestMethod]
	public void ParseSubfolders_FieldAccessException_DotExt()
	{
		const string structure = "/.ext";
		var sut = new StructureService(new FakeIStorage(), structure);
		Assert.ThrowsException<FieldAccessException>(() => sut.ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local)));
	}

	[TestMethod]
	public void ParseSubfolders_FieldAccessException_DoesNotStartWithSlash()
	{
		const string structure = "test/on";
		var sut = new StructureService(new FakeIStorage(), structure);
		Assert.ThrowsException<FieldAccessException>(() => sut.ParseSubfolders(
			new DateTime(2020, 01, 01,
				01, 01, 01, DateTimeKind.Local)));
		// ExpectedException
	}

	[TestMethod]
	public void ParseSubfolders_Int_Null()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

		var result = new StructureService(new FakeIStorage(), structure).ParseSubfolders(null);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void ParseSubfolders_Int_RelativeToday()
	{
		const string structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
		var result = new StructureService(new FakeIStorage(), structure).ParseSubfolders(0);
		Assert.IsTrue(result?.Contains(DateTime.Now.ToString("yyyy_MM_dd")));
	}
}
