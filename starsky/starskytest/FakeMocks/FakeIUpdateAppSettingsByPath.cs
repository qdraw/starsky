#nullable enable
using System.Threading.Tasks;
using starsky.feature.settings.Interfaces;
using starsky.feature.settings.Models;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks;

public class FakeIUpdateAppSettingsByPath : IUpdateAppSettingsByPath
{
	private readonly UpdateAppSettingsStatusModel? _updateAppSettingsStatusModel;

	public FakeIUpdateAppSettingsByPath(UpdateAppSettingsStatusModel? updateAppSettingsStatusModel = null)
	{
		_updateAppSettingsStatusModel = updateAppSettingsStatusModel;
	}
	
	public Task<UpdateAppSettingsStatusModel> UpdateAppSettingsAsync(AppSettingsTransferObject appSettingTransferObject)
	{
		if ( _updateAppSettingsStatusModel == null )
		{
			return Task.FromResult(new UpdateAppSettingsStatusModel
			{
				Message = "Ok",
				StatusCode = 200
			});
		}
		
		return Task.FromResult(_updateAppSettingsStatusModel);
	}
}
