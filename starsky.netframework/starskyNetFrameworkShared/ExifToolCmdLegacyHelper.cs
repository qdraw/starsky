using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;

namespace starskyNetFrameworkShared
{
	public class ExifToolCmdLegacyHelper
	{
		private readonly AppSettings _appSettings;

		public ExifToolCmdLegacyHelper(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		
		public void XmpLegacySync(IEnumerable<string> listOfSubPaths)
		{
			if ( !_appSettings.ExifToolImportXmpCreate )
			{
				System.Console.WriteLine("ExifToolImportXmpCreate = disabled");
				return;
			}

			foreach ( var subPath in listOfSubPaths )
			{
				XmpLegacySync(subPath);
			}
		}

		/// <summary>
		/// Add a .xmp sidecar file
		/// </summary>
		/// <param name="subPath"></param>
		/// <returns></returns>
		public string XmpLegacySync(string subPath)
		{
			// only for raw files
			if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) ) return string.Empty;

			var withXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);

			var fullXmpPath = _appSettings.DatabasePathToFilePath(withXmp,false);
			var fullPathImage = _appSettings.DatabasePathToFilePath(subPath);

			// only for files that not exist yet
			if ( new StorageHostFullPathFilesystem().IsFolderOrFile(fullXmpPath) != 
			     FolderOrFileModel.FolderOrFileTypeList.Deleted) return string.Empty;
			
			
			return new ExifToolLegacy(_appSettings).Run(
				" -overwrite_original -TagsFromFile \"" + fullPathImage + "\"",
				"\"" + fullXmpPath + "\"" + " -Orientation=");

		}
	}
}
