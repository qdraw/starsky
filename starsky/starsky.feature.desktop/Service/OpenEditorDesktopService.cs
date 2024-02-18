using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.injection;
using starsky.foundation.native.OpenApplicationNative.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

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

	public async Task<(bool?, string)> OpenAsync(string f, bool collections)
	{
		var inputFilePaths = PathHelper.SplitInputFilePaths(f);
		return await OpenAsync(inputFilePaths.ToList(), collections);
	}

	private async Task<(bool?, string)> OpenAsync(List<string> subPaths, bool collections)
	{
		if ( _appSettings.UseLocalDesktop == false )
		{
			return ( null, "UseLocalDesktop is false" );
		}

		if ( subPaths.Count == 0 )
		{
			return ( false, "No files selected" );
		}

		var subPathAndImageFormatList =
			await _openEditorPreflight.PreflightAsync(subPaths, collections);

		var (openDefaultList, openWithEditorList) = FilterList(subPathAndImageFormatList);
		_openApplicationNativeService.OpenDefault(openDefaultList);
		_openApplicationNativeService.OpenApplicationAtUrl(openWithEditorList);


		return ( false, "TODO: Open Editor" );
	}

	// [Obsolete("replace with PreflightAsync")]
	// private List<PathImageFormatExistsAppPathModel> Preflight(List<string> subPaths)
	// {
	// 	var subPathAndImageFormatList = new List<PathImageFormatExistsAppPathModel>();
	// 	foreach ( var subPath in subPaths )
	// 	{
	// 		if ( _iStorage.ExistFile(subPath) )
	// 		{
	// 			subPathAndImageFormatList.Add(new PathImageFormatExistsAppPathModel
	// 			{
	// 				AppPath = string.Empty,
	// 				Exists = false,
	// 				ImageFormat = ExtensionRolesHelper.ImageFormat.notfound,
	// 				SubPath = subPath
	// 			});
	// 		}
	//
	// 		var first50BytesStream = _iStorage.ReadStream(subPath, 50);
	// 		var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);
	// 		first50BytesStream.Dispose();
	//
	// 		var appSettingsDefaultEditor =
	// 			_appSettings.DefaultDesktopEditor.Find(p => p.ImageFormats.Contains(imageFormat));
	//
	// 		subPathAndImageFormatList.Add(new PathImageFormatExistsAppPathModel
	// 		{
	// 			AppPath = appSettingsDefaultEditor?.ApplicationPath ?? string.Empty,
	// 			Exists = true,
	// 			ImageFormat = imageFormat,
	// 			SubPath = subPath,
	// 			FullFilePath = _appSettings.DatabasePathToFilePath(subPath)
	// 		});
	// 	}
	//
	// 	return subPathAndImageFormatList;
	// }

	/// <summary>
	/// Filter the list
	/// First is the list with the files that exists and AppPath is set
	/// Second is the list with the files that exists but AppPath is not set
	/// </summary>
	/// <param name="subPathAndImageFormatList"></param>
	/// <returns></returns>
	private static (List<string>, List<(string SubPath, string AppPath)>) FilterList(
		List<PathImageFormatExistsAppPathModel> subPathAndImageFormatList)
	{
		// TODO: MAP to fullFilePaths
		var appPathList = subPathAndImageFormatList
			.Where(p => p.Exists && !string.IsNullOrEmpty(p.AppPath))
			.Select(p => p.SubPath).ToList();
		var noAppPathList = subPathAndImageFormatList
			.Where(p => p.Exists && string.IsNullOrEmpty(p.AppPath))
			.Select(p => ( p.SubPath, p.AppPath )).ToList();
		return ( appPathList, noAppPathList );
	}
}
