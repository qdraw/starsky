using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks;

public class FakeIImport : IImport
{
	private readonly ISelectorStorage _selectorStorage;

	public FakeIImport(ISelectorStorage selectorStorage)
	{
		_selectorStorage = selectorStorage;
	}

	public List<ImportIndexItem> HistoryList { get; set; } = new();

	public List<ImportIndexItem> PreflightList { get; set; } = new();

	public async Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList,
		ImportSettingsModel importSettings)
	{
		var preflight = await Preflight(inputFullPathList.ToList(), importSettings);
		HistoryList.AddRange(preflight);
		return preflight;
	}

	public Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
		ImportSettingsModel importSettings)
	{
		var results = new List<ImportIndexItem>();
		foreach ( var inputFileFullPath in fullFilePathsList )
		{
			// if the item fails
			var importIndexFileError = new ImportIndexItem
			{
				FilePath = "/" + FilenamesHelper.GetFileName(inputFileFullPath),
				SourceFullFilePath = "~/temp/test",
				FileHash = "FAKE",
				MakeModel = "added if the item fails",
				Status = ImportStatus.FileError
			};

			// Check if extension is correct
			if ( !ExtensionRolesHelper.IsExtensionSyncSupported(inputFileFullPath) )
			{
				results.Add(importIndexFileError);
			}

			// Check if the file is correct
			var imageFormat = new ExtensionRolesHelper(new FakeIWebLogger()).GetImageFormat(
				_selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem)
					.ReadStream(inputFileFullPath, 160));

			if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains($"{imageFormat}") )
			{
				results.Add(importIndexFileError);
			}

			results.Add(new ImportIndexItem
			{
				Id = 4,
				SourceFullFilePath = inputFileFullPath,
				FilePath = inputFileFullPath,
				Status = ImportStatus.Ok,
				FileHash = "FAKE",
				MakeModel = "added okay",
				FileIndexItem =
					new FileIndexItem { FileHash = "FAKE_OK", FilePath = inputFileFullPath }
			});
		}

		PreflightList.AddRange(results);
		return Task.FromResult(results);
	}

	public List<string> ImportTo(string inputFullPathList, ImportSettingsModel importSettings)
	{
		throw new NotImplementedException();
	}

	public List<string> Import(string inputFullPathList, ImportSettingsModel importSettings)
	{
		HistoryList = Preflight(new List<string> { inputFullPathList }, importSettings).Result;
		return new List<string> { inputFullPathList };
	}


	public List<ImportIndexItem> History()
	{
		return HistoryList;
	}
}
