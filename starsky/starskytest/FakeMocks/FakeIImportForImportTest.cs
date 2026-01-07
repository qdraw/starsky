using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;

namespace starskytest.FakeMocks;

public class FakeIImportForImportTest : IImport
{
	public Func<IEnumerable<string>, ImportSettingsModel, List<ImportIndexItem>>? ImporterFunc
	{
		get;
		set;
	}

	public bool ThrowOnImport { get; set; }
	public List<(IEnumerable<string> paths, ImportSettingsModel settings)> Calls { get; } = new();

	public Task<List<ImportIndexItem>> Preflight(List<string> fullFilePathsList,
		ImportSettingsModel importSettings)
	{
		return Task.FromResult(new List<ImportIndexItem>());
	}

	public Task<List<ImportIndexItem>> Importer(IEnumerable<string> inputFullPathList,
		ImportSettingsModel importSettings)
	{
		Calls.Add(( inputFullPathList, importSettings ));
		if ( ThrowOnImport )
		{
			throw new Exception("Simulated import exception");
		}

		return Task.FromResult(ImporterFunc?.Invoke(inputFullPathList, importSettings) ??
		                       new List<ImportIndexItem>());
	}
}
