using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Services;

[TestClass]
public sealed class ExifCopyTest
{
	private readonly AppSettings _appSettings = new();

	private (ExifCopy, FakeIStorage) CreateSut()
	{
		var folderPaths = new List<string> { "/" };
		var inputSubPaths = new List<string> { "/test.dng" };
		var storage =
			new FakeIStorage(folderPaths, inputSubPaths,
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var fakeReadMeta = new ReadMeta(storage, _appSettings,
			null, new FakeIWebLogger());
		var fakeExifTool = new FakeExifTool(storage, _appSettings);

		var sut = new ExifCopy(storage, storage, fakeExifTool, fakeReadMeta,
			new FakeIThumbnailQuery(),
			new FakeIWebLogger());
		return ( sut, storage );
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_CopyExifPublish()
	{
		var folderPaths = new List<string> { "/" };
		var inputSubPaths = new List<string> { "/test.jpg" };
		var storage =
			new FakeIStorage(folderPaths, inputSubPaths,
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var fakeReadMeta = new ReadMeta(storage, _appSettings, null, new FakeIWebLogger());
		var fakeExifTool = new FakeExifTool(storage, _appSettings);
		var helperResult = await new ExifCopy(storage, storage, fakeExifTool,
				fakeReadMeta, new FakeIThumbnailQuery(), new FakeIWebLogger())
			.CopyExifPublish("/test.jpg", "/test2");
		Assert.Contains("HistorySoftwareAgent", helperResult);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_XmpSync()
	{
		var (sut, _) = CreateSut();

		var helperResult = await sut.XmpSync("/test.dng");
		Assert.AreEqual("/test.xmp", helperResult);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_XmpCreate()
	{
		var (sut, storage) = CreateSut();
		var job = sut.XmpCreate("/test.xmp");
		Assert.IsTrue(job);

		var result =
			await StreamToStringHelper.StreamToStringAsync(storage.ReadStream("/test.xmp"));
		Assert.AreEqual("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Starsky'>\n" +
		                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>\n</rdf:RDF>\n</x:xmpmeta>",
			result);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_XmpCreate_Twice()
	{
		var (sut, storage) = CreateSut();

		// Run twice, that's my only way to test if the file is already there
		sut.XmpCreate("/test.xmp");
		var job2 = sut.XmpCreate("/test.xmp");

		Assert.IsFalse(job2);

		var result =
			await StreamToStringHelper.StreamToStringAsync(storage.ReadStream("/test.xmp"));
		Assert.AreEqual("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Starsky'>\n" +
		                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>\n</rdf:RDF>\n</x:xmpmeta>",
			result);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_XmpSync_Twice()
	{
		var (sut, storage) = CreateSut();

		// Run twice, that's my only way to test if the file is already there
		sut.XmpCreate("/test.xmp");

		var job1 = await sut.XmpSync("/test.dng");
		var job2 = await sut.XmpSync("/test.dng");

		Assert.AreEqual("/test.xmp", job1);
		Assert.AreEqual("/test.xmp", job2);
		Assert.AreEqual(job1, job2);

		var result =
			await StreamToStringHelper.StreamToStringAsync(storage.ReadStream("/test.xmp"));
		Assert.AreEqual("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Starsky'>\n" +
		                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>\n</rdf:RDF>\n</x:xmpmeta>",
			result);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_XmpCreate_NotFound()
	{
		var (sut, _) = CreateSut();
		var job = await sut.XmpSync("/not-found.dng");
		Assert.AreEqual("/not-found.xmp", job);
	}

	[TestMethod]
	public async Task ExifToolCmdHelper_TestForFakeExifToolInjection()
	{
		var (sut, storage) = CreateSut();

		await sut.XmpSync("/test.dng");

		Assert.IsTrue(storage.ExistFile("/test.xmp"));
		var xmpContentReadStream = storage.ReadStream("/test.xmp");
		var xmpContent = await StreamToStringHelper.StreamToStringAsync(xmpContentReadStream);

		// Those values are injected by fakeExifTool
		Assert.Contains("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Image::ExifTool 11.30'>",
			xmpContent);
		Assert.Contains("<rdf:li>test</rdf:li>", xmpContent);
	}
}
