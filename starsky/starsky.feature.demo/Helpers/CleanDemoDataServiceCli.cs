using System.Threading.Tasks;
using starsky.feature.demo.Services;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.feature.demo.Helpers;

public class CleanDemoDataServiceCli
{
	private readonly AppSettings _appSettings;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IStorage _hostStorage;
	private readonly IStorage _subPathStorage;
	private readonly IWebLogger _webLogger;
	private readonly ISynchronize _sync;
	private readonly IConsole _console;

	public CleanDemoDataServiceCli(AppSettings appSettings,
		IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage,
		IWebLogger webLogger, IConsole console,
		ISynchronize sync)
	{
		_appSettings = appSettings;
		_httpClientHelper = httpClientHelper;
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_subPathStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_webLogger = webLogger;
		_console = console;
		_sync = sync;
	}

	public async Task SeedCli(string[] args)
	{
		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);

		if ( ArgsHelper.NeedHelp(args) )
		{
			_appSettings.ApplicationType = AppSettings.StarskyAppType.DemoSeed;
			new ArgsHelper(_appSettings, _console).NeedHelpShowDialog();
			return;
		}

		await CleanDemoDataService.DownloadAsync(_appSettings, _httpClientHelper, _hostStorage, _subPathStorage, _webLogger);
		await _sync.Sync("/");
	}
}
