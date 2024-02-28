using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;

namespace starskytest.FakeMocks;

public class FakeIOpenEditorPreflight : IOpenEditorPreflight
{
	private readonly List<PathImageFormatExistsAppPathModel> _content;

	public FakeIOpenEditorPreflight(List<PathImageFormatExistsAppPathModel> content)
	{
		_content = content;
	}

	public Task<List<PathImageFormatExistsAppPathModel>> PreflightAsync(List<string> inputFilePaths,
		bool collections)
	{
		return Task.FromResult(_content);
	}
}
