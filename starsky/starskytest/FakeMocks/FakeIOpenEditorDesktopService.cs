using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeMocks;

public class FakeIOpenEditorDesktopService : IOpenEditorDesktopService
{
	private readonly bool _isEnabled;

	public FakeIOpenEditorDesktopService(bool isEnabled = true)
	{
		_isEnabled = isEnabled;
	}

	public bool IsEnabled()
	{
		return _isEnabled;
	}

	public async Task<(bool?, string, List<PathImageFormatExistsAppPathModel>)> OpenAsync(string f,
		bool collections)
	{
		await Task.Yield();

		var list = new List<PathImageFormatExistsAppPathModel>
		{
			new PathImageFormatExistsAppPathModel
			{
				AppPath = "test",
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
				SubPath = "/test.jpg",
				FullFilePath = "/test.jpg"
			}
		};

		return ( _isEnabled, "Opened", list );
	}
}
