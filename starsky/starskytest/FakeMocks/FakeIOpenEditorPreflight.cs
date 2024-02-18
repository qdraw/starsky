using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;

namespace starskytest.FakeMocks;

public class FakeIOpenEditorPreflight : IOpenEditorPreflight
{
	public Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(List<string> inputFilePaths,
		bool collections)
	{
		return Task.FromResult(new List<PathImageFormatExistsAppPathModel>());
	}
}
