using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public sealed class ImportIndexJsonServiceTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task ExportAsync_IncludesStructureAndData()
	{
		var tempFile = Path.Combine(Path.GetTempPath(),
			$"starsky-importindex-export-{Guid.NewGuid():N}.json");
		try
		{
			var appSettings = new AppSettings
			{
				Structure = new AppSettingsStructureModel("/yyyy/MM/{filenamebase}.ext")
			};
			var query = new FakeIImportQuery(["hash-a", "hash-b"]);
			var fakeStorage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(fakeStorage);
			var sut = new ImportIndexJsonService(query, appSettings, selectorStorage);

			await sut.ExportAsync(tempFile);

			Assert.IsTrue(fakeStorage.ExistFile(tempFile));
			await using var rs = fakeStorage.ReadStream(tempFile);
			using var sr = new StreamReader(rs);
			var json = await sr.ReadToEndAsync(TestContext.CancellationToken);
			Assert.Contains("\"structure\"", json);
			Assert.Contains("\"items\"", json);
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}

	[TestMethod]
	public async Task ImportAsync_SkipsExistingByFileHash()
	{
		var tempFile = Path.Combine(Path.GetTempPath(),
			$"starsky-importindex-import-{Guid.NewGuid():N}.json");
		try
		{
			var model = new ImportIndexJsonContainer
			{
				Structure = new AppSettingsStructureModel(),
				Items =
				[
					new ImportIndexItem { FileHash = "exists", FilePath = "/a.jpg" },
					new ImportIndexItem { FileHash = "new", FilePath = "/b.jpg" }
				]
			};

			var json = JsonSerializer.Serialize(model, DefaultJsonSerializer.CamelCase);
			var fakeStorage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(fakeStorage);
			await fakeStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(json), tempFile);

			var query = new FakeIImportQuery(["exists"]);
			var sut = new ImportIndexJsonService(query, new AppSettings(), selectorStorage);

			var result = await sut.ImportAsync(tempFile);

			Assert.HasCount(2, result);
			Assert.AreEqual(ImportStatus.IgnoredAlreadyImported,
				result.Single(p => p.FileHash == "exists").Status);
			Assert.AreEqual(ImportStatus.Ok, result.Single(p => p.FileHash == "new").Status);
			Assert.IsTrue(await query.IsHashInImportDbAsync("new"));
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}

	[TestMethod]
	public async Task ImportAsync_ThrowsOnMissingStructureOrItemsSection()
	{
		var tempFile = Path.Combine(Path.GetTempPath(),
			$"starsky-importindex-invalid-{Guid.NewGuid():N}.json");
		try
		{
			var fakeStorage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(fakeStorage);
			await fakeStorage.WriteStreamAsync(
				StringToStreamHelper.StringToStream("{\"items\":[]}"), 
				tempFile);

			var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
				selectorStorage);

			await Assert.ThrowsExactlyAsync<InvalidDataException>(async () =>
				await sut.ImportAsync(tempFile));
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}

	[TestMethod]
	public async Task ExportAsync_ThrowsOnEmptyPath()
	{
		var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()));

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await sut.ExportAsync(string.Empty));
	}

	[TestMethod]
	public async Task ExportAsync_WithDirectory_CreatesDirectory()
	{
		var tempDirectory = $"/tmp-{Guid.NewGuid():N}";
		var outputPath = Path.Combine(tempDirectory, "import-index.json");
		var fakeStorage = new FakeIStorage();
		var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
			new FakeSelectorStorage(fakeStorage));

		await sut.ExportAsync(outputPath);

		Assert.IsTrue(fakeStorage.ExistFolder(tempDirectory));
		Assert.IsTrue(fakeStorage.ExistFile(outputPath));
	}

	[TestMethod]
	public async Task ImportAsync_ThrowsOnEmptyPath()
	{
		var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()));

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await sut.ImportAsync(string.Empty));
	}

	[TestMethod]
	public async Task ImportAsync_ThrowsOnFileNotFound()
	{
		var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
			new FakeSelectorStorage(new FakeIStorage()));

		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await sut.ImportAsync("/missing.json"));
	}

	[TestMethod]
	public async Task ImportAsync_MarksMissingFileHashAsFileError()
	{
		var tempFile = Path.Combine(Path.GetTempPath(),
			$"starsky-importindex-nohash-{Guid.NewGuid():N}.json");
		try
		{
			var model = new ImportIndexJsonContainer
			{
				Structure = new AppSettingsStructureModel(),
				Items =
				[
					new ImportIndexItem { FileHash = string.Empty, FilePath = "/nohash.jpg" }
				]
			};
			var fakeStorage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(fakeStorage);
			await fakeStorage.WriteStreamAsync(
				StringToStreamHelper.StringToStream(
					JsonSerializer.Serialize(model, DefaultJsonSerializer.CamelCase)),
				tempFile);

			var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
				selectorStorage);
			var result = await sut.ImportAsync(tempFile);

			Assert.AreEqual(ImportStatus.FileError, result.Single().Status);
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}

	[TestMethod]
	public async Task ImportAsync_ThrowsOnNullItemsAfterDeserialization()
	{
		var tempFile = Path.Combine(Path.GetTempPath(),
			$"starsky-importindex-nullitems-{Guid.NewGuid():N}.json");
		try
		{
			var fakeStorage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(fakeStorage);
			await fakeStorage.WriteStreamAsync(
				StringToStreamHelper.StringToStream("{\"structure\":{},\"items\":null}"),
				tempFile);

			var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings(),
				selectorStorage);

			await Assert.ThrowsExactlyAsync<InvalidDataException>(async () =>
				await sut.ImportAsync(tempFile));
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}
}
