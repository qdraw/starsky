using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks;

public class FakeIImportIndexJsonService : IImportIndexJsonService
{
	public string ExportPath { get; private set; } = string.Empty;
	public string ImportPath { get; private set; } = string.Empty;
	public List<ImportIndexItem> ImportResult { get; set; } = [];

	public Task<string> ExportAsync(string outputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem)
	{
		ExportPath = outputJsonPath;
		return Task.FromResult(outputJsonPath);
	}

	public Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath, 
		SelectorStorage.StorageServices type =  SelectorStorage.StorageServices.HostFilesystem)
	{
		ImportPath = inputJsonPath;
		return Task.FromResult(ImportResult);
	}
}
