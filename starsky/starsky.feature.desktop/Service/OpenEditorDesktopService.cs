using System.ComponentModel.Design;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;
using starsky.foundation.injection;
using starsky.foundation.native.OpenApplicationNative.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorDesktopService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorDesktopService : IOpenEditorDesktopService
{
	private readonly AppSettings _appSettings;
	private readonly IOpenApplicationNativeService _openApplicationNativeService;
	private readonly IStorage _iStorage;

	public OpenEditorDesktopService(AppSettings appSettings,
		IOpenApplicationNativeService openApplicationNativeService,
		ISelectorStorage selectorStorage)
	{
		_appSettings = appSettings;
		_openApplicationNativeService = openApplicationNativeService;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	}

	public (bool?, string) Open(List<string> subPaths)
	{
		if ( _appSettings.UseLocalDesktop == false )
		{
			return ( null, "UseLocalDesktop is false" );
		}

		if ( subPaths.Count == 0 )
		{
			return ( false, "No files selected" );
		}

		var subPathAndImageFormat = new List<PathImageFormatExistsAppPathModel>();
		foreach ( var subPath in subPaths )
		{
			if ( _iStorage.ExistFile(subPath) )
			{
				subPathAndImageFormat.Add(new PathImageFormatExistsAppPathModel
				{
					AppPath = string.Empty,
					Exists = false,
					ImageFormat = ExtensionRolesHelper.ImageFormat.notfound,
					SubPath = subPath
				});
			}

			var first50BytesStream = _iStorage.ReadStream(subPath, 50);
			var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);
			first50BytesStream.Dispose();

			var appSettingsDefaultEditor =
				_appSettings.DefaultDesktopEditor.Find(p => p.ImageFormats.Contains(imageFormat));

			subPathAndImageFormat.Add(new PathImageFormatExistsAppPathModel
			{
				AppPath = appSettingsDefaultEditor?.ApplicationPath ?? string.Empty,
				Exists = true,
				ImageFormat = imageFormat,
				SubPath = subPath
			});
		}


		return ( false, "TODO: Open Editor" );
	}
}
