using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.export.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeIExport : IExport
{
	private readonly Dictionary<string, bool> _status;

	public FakeIExport(Dictionary<string, bool> status)
	{
		_status = status;
	}
	public Task CreateZip(List<FileIndexItem> fileIndexResultsList, bool thumbnail,
		string zipOutputFileName)
	{
		throw new NotImplementedException();
	}

	public Task<Tuple<string, List<FileIndexItem>>> PreflightAsync(
		string[] inputFilePaths, bool collections = true,
		bool thumbnail = false)
	{
		throw new NotImplementedException();
	}

	public Tuple<bool?, string?> StatusIsReady(string zipOutputFileName)
	{
		var result = _status.FirstOrDefault(p => p.Key == zipOutputFileName);
		return new Tuple<bool?, string?>(result.Value, result.Key);
	}
}
