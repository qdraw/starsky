using System.Threading.Tasks;
using starsky.feature.webftppublish.FtpAbstractions.Interfaces;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webftppublish.Helpers;

public class WebFtpCli
{
	private readonly AppSettings _appSettings;
	private readonly ArgsHelper _argsHelper;
	private readonly IConsole _console;
	private readonly IWebLogger _logger;
	private readonly ISelectorStorage _selectorStorage;
	private readonly IFtpWebRequestFactory _webRequestFactory;

	public WebFtpCli(AppSettings appSettings, ISelectorStorage selectorStorage,
		IConsole console,
		IFtpWebRequestFactory webRequestFactory, IWebLogger logger)
	{
		_appSettings = appSettings;
		_console = console;
		_argsHelper = new ArgsHelper(_appSettings, console);
		_selectorStorage = selectorStorage;
		_webRequestFactory = webRequestFactory;
		_logger = logger;
	}

	public async Task RunAsync(string[] args)
	{
		_appSettings.Verbose = ArgsHelper.NeedVerbose(args);

		if ( ArgsHelper.NeedHelp(args) )
		{
			_appSettings.ApplicationType = AppSettings.StarskyAppType.WebFtp;
			_argsHelper.NeedHelpShowDialog();
			return;
		}

		var inputFullFileDirectoryOrZip = new ArgsHelper(_appSettings)
			.GetPathFormArgs(args, false);

		// check if settings are valid
		if ( _appSettings.PublishProfilesRemote.GetFtpById(ArgsHelper.GetProfile(args)).Count == 0 )
		{
			_logger.LogError(
				"Please update the PublishProfilesRemote settings in appsettings.json");
			return;
		}

		var ftpService = new FtpService(_appSettings, _selectorStorage,
			_console, _webRequestFactory, _logger);

		var manifest = await ftpService.IsValidZipOrFolder(inputFullFileDirectoryOrZip);
		if ( manifest == null )
		{
			_logger.LogError("Invalid input, please provide a valid zip file or folder");
			return;
		}

		var ftpResult = ftpService.Run(inputFullFileDirectoryOrZip, ArgsHelper.GetProfile(args),
			manifest.Slug, manifest.Copy);

		if ( !ftpResult )
		{
			_console.WriteLine("Ftp copy failed");
			return;
		}

		_console.WriteLine($"Ftp copy successful done: {manifest.Slug}");
	}
}
