using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.import.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public class ImportJsonCliTests
{
	[TestMethod]
	public async Task ImporterCli_ImportIndexJson_Export_Command()
	{
		var fakeImportIndexJsonService = new FakeIImportIndexJsonService();
		var sut = new ImportJsonCli(fakeImportIndexJsonService, new FakeIWebLogger());

		var result =
			await sut.ImportExportByArgs(["--importindex-export-json", "/tmp/export.json"]);

		Assert.IsTrue(result);
		Assert.AreEqual("/tmp/export.json", fakeImportIndexJsonService.ExportPath);
	}

	[TestMethod]
	public async Task ImporterCli_ImportIndexJson_Import_Command()
	{
		var fakeImportIndexJsonService = new FakeIImportIndexJsonService
		{
			ImportResult =
			[
				new ImportIndexItem { FileHash = "a", Status = ImportStatus.Ok },
				new ImportIndexItem { FileHash = "b", Status = ImportStatus.IgnoredAlreadyImported }
			]
		};

		var sut = new ImportJsonCli(fakeImportIndexJsonService, new FakeIWebLogger());

		var result =
			await sut.ImportExportByArgs(["--importindex-import-json", "/tmp/import.json"]);

		Assert.IsTrue(result);
		Assert.AreEqual("/tmp/import.json", fakeImportIndexJsonService.ImportPath);
	}

	[TestMethod]
	public async Task ImporterCli_ImportIndexJson_Export_Command_NoService()
	{
		var sut = new ImportJsonCli(null!, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<NullReferenceException>(async () =>
			await sut.ImportExportByArgs(["--importindex-export-json", "/tmp/export.json"]));
	}

	[TestMethod]
	public async Task ImporterCli_ImportIndexJson_Import_Command_NoService()
	{
		var sut = new ImportJsonCli(null!, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<NullReferenceException>(async () =>
			await sut.ImportExportByArgs(["--importindex-import-json", "/tmp/import.json"]));
	}

	[TestMethod]
	public async Task ImporterCli_ImportIndexJson_Import_Command_FailedStatus()
	{
		var fakeImportIndexJsonService = new FakeIImportIndexJsonService
		{
			ImportResult =
			[
				new ImportIndexItem { FileHash = "a", Status = ImportStatus.FileError }
			]
		};

		var sut = new ImportJsonCli(fakeImportIndexJsonService, new FakeIWebLogger());

		var result =
			await sut.ImportExportByArgs(["--importindex-import-json", "/tmp/import.json"]);

		Assert.IsFalse(result);
	}
}
