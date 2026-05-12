using System;
using System.Collections.Generic;
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
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public sealed class ImportIndexJsonServiceTest
{
	[TestMethod]
	public async Task ExportAsync_IncludesStructureAndData()
	{
		var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-importindex-export-{Guid.NewGuid():N}.json");
		try
		{
			var appSettings = new AppSettings
			{
				Structure = new AppSettingsStructureModel("/yyyy/MM/{filenamebase}.ext")
			};
			var query = new FakeIImportQuery(["hash-a", "hash-b"]);
			var sut = new ImportIndexJsonService(query, appSettings);

			await sut.ExportAsync(tempFile);

			Assert.IsTrue(File.Exists(tempFile));
			var json = await File.ReadAllTextAsync(tempFile);
			Assert.IsTrue(json.Contains("\"structure\""));
			Assert.IsTrue(json.Contains("\"items\""));
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
		var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-importindex-import-{Guid.NewGuid():N}.json");
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
			await File.WriteAllTextAsync(tempFile, json);

			var query = new FakeIImportQuery(["exists"]);
			var sut = new ImportIndexJsonService(query, new AppSettings());

			var result = await sut.ImportAsync(tempFile);

			Assert.AreEqual(2, result.Count);
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
		var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-importindex-invalid-{Guid.NewGuid():N}.json");
		try
		{
			await File.WriteAllTextAsync(tempFile, "{\"items\":[]}");

			var sut = new ImportIndexJsonService(new FakeIImportQuery(), new AppSettings());

			await Assert.ThrowsExactlyAsync<InvalidDataException>(async () => await sut.ImportAsync(tempFile));
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