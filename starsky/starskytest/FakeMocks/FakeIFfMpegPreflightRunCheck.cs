using System.Threading.Tasks;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIFfMpegPreflightRunCheck : IFfMpegPreflightRunCheck
{
	private readonly AppSettings? _appSettings;
	private readonly IStorage? _storage;

	public FakeIFfMpegPreflightRunCheck(IStorage? storage = null, AppSettings? appSettings = null)
	{
		_storage = storage;
		_appSettings = appSettings;
	}

	public async Task<bool> TryRun()
	{
		var currentArchitecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		return await TryRun(currentArchitecture);
	}

	public Task<bool> TryRun(string currentArchitecture)
	{
		if ( _appSettings == null || _storage == null )
		{
			return Task.FromResult(false);
		}

		var exePath = new FfmpegExePath(_appSettings).GetExePath(currentArchitecture);
		return Task.FromResult(_storage.ExistFile(exePath));
	}
}
