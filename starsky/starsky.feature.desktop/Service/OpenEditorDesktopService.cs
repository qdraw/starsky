using System.Runtime.CompilerServices;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.native.OpenApplicationNative.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorDesktopService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorDesktopService : IOpenEditorDesktopService
{
	private readonly AppSettings _appSettings;
	private readonly IOpenApplicationNativeService _openApplicationNativeService;
	private readonly IOpenEditorPreflight _openEditorPreflight;

	public OpenEditorDesktopService(AppSettings appSettings,
		IOpenApplicationNativeService openApplicationNativeService,
		IOpenEditorPreflight openEditorPreflight)
	{
		_appSettings = appSettings;
		_openApplicationNativeService = openApplicationNativeService;
		_openEditorPreflight = openEditorPreflight;
	}

	public async Task<(bool?, string, List<PathImageFormatExistsAppPathModel>)> OpenAsync(string f,
		bool collections)
	{
		var inputFilePaths = PathHelper.SplitInputFilePaths(f);
		return await OpenAsync(inputFilePaths.ToList(), collections);
	}

	internal async Task<(bool?, string, List<PathImageFormatExistsAppPathModel>)> OpenAsync(
		List<string> subPaths, bool collections)
	{
		if ( _appSettings.UseLocalDesktop == false )
		{
			return ( null, "UseLocalDesktop feature toggle is disabled", [] );
		}

		var subPathAndImageFormatList =
			await _openEditorPreflight.PreflightAsync(subPaths, collections);

		if ( subPathAndImageFormatList.Count == 0 )
		{
			return ( false, "No files selected", [] );
		}

		var (openDefaultList, openWithEditorList) =
			FilterListOpenDefaultEditorAndSpecificEditor(subPathAndImageFormatList);

		_openApplicationNativeService.OpenDefault(openDefaultList);
		_openApplicationNativeService.OpenApplicationAtUrl(openWithEditorList);

		return ( true, "Opened", subPathAndImageFormatList );
	}

	/// <summary>
	/// Filter the list
	/// First is the list with the files that exists and AppPath is set
	/// Second is the list with the files that exists but AppPath is not set
	/// </summary>
	/// <param name="subPathAndImageFormatList"></param>
	/// <returns></returns>
	private static (List<string>, List<(string FullFilePath, string AppPath)>)
		FilterListOpenDefaultEditorAndSpecificEditor(
			IReadOnlyCollection<PathImageFormatExistsAppPathModel> subPathAndImageFormatList)
	{
		var appPathList = subPathAndImageFormatList
			.Where(p => p.Status == FileIndexItem.ExifStatus.Ok &&
			            string.IsNullOrEmpty(p.AppPath))
			.Select(p => p.FullFilePath).ToList();
		var noAppPathList = subPathAndImageFormatList
			.Where(p => p.Status == FileIndexItem.ExifStatus.Ok &&
			            !string.IsNullOrEmpty(p.AppPath))
			.Select(p => ( p.FullFilePath, p.AppPath )).ToList();
		return ( appPathList, noAppPathList );
	}
}
