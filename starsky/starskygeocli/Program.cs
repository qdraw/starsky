using System;
using System.Collections.Generic;
using System.Linq;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starskycore.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.writemeta.Services;

namespace starskyGeoCli
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			new ArgsHelper().SetEnvironmentByArgs(args);

			var startupHelper = new ConfigCliAppsStartupHelper();
			var appSettings = startupHelper.AppSettings();

			// Use args in application
			appSettings.Verbose = new ArgsHelper().NeedVerbose(args);

			// When there is no input show help
			if ( new ArgsHelper().NeedHelp(args) ||
			     ( new ArgsHelper(appSettings).GetPathFormArgs(args, false).Length <= 1
			       && new ArgsHelper().GetSubpathFormArgs(args).Length <= 1
			       && new ArgsHelper(appSettings).GetRelativeValue(args) == null ) )
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Geo;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}

			// Using both options
			string inputPath;
			// -s = if subpath || -p is path
			if ( new ArgsHelper(appSettings).IsSubPathOrPath(args) )
			{
				inputPath = appSettings.DatabasePathToFilePath(
					new ArgsHelper(appSettings).GetSubpathFormArgs(args)
				);
			}
			else
			{
				inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args, false);
			}

			// overwrite subPath with relative days
			// use -g or --SubPathRelative to use it.
			// envs are not supported
			var getSubPathRelative = new ArgsHelper(appSettings).GetRelativeValue(args);
			if (getSubPathRelative != null)
			{
				var dateTime = DateTime.Now.AddDays(( double ) getSubPathRelative);
				inputPath = new StructureService(startupHelper.SubPathStorage(), appSettings.Structure)
					.ParseSubfolders(dateTime);
			}
			
			// used in this session to find the files back
			appSettings.StorageFolder = inputPath;
			var storage = startupHelper.SubPathStorage();
			
			if ( inputPath == null || storage.IsFolderOrFile("/") == FolderOrFileModel.FolderOrFileTypeList.Deleted )
			{
				Console.WriteLine(
					$"Folder location is not found \nPlease try the `-h` command to get help ");
				return;
			}

			// use relative to StorageFolder
			var listOfFiles = storage.GetAllFilesInDirectory("/")
				.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

			var fileIndexList = startupHelper.ReadMeta()
				.ReadExifAndXmpFromFileAddFilePathHash(listOfFiles);

			var toMetaFilesUpdate = new List<FileIndexItem>();
			if ( new ArgsHelper().GetIndexMode(args) )
			{
				Console.WriteLine($"CameraTimeZone: {appSettings.CameraTimeZone}");
				Console.WriteLine($"Folder: {inputPath}");

				toMetaFilesUpdate =
					new GeoIndexGpx(appSettings, storage)
												.LoopFolder(fileIndexList);
				
				Console.Write("¬");
				new GeoLocationWrite(appSettings, startupHelper.ExifTool(), 
					startupHelper.SubPathStorage(), startupHelper.ThumbnailStorage()).
					LoopFolder(toMetaFilesUpdate, 
					false
					);
				Console.Write("(gps added)");
			}

			fileIndexList =
				new GeoReverseLookup(appSettings).LoopFolderLookup(fileIndexList,
					new ArgsHelper().GetAll(args));
			if ( fileIndexList.Count >= 1 )
			{
				Console.Write("~ Add city, state and country info ~");
				new GeoLocationWrite(appSettings, startupHelper.ExifTool(), startupHelper.SubPathStorage(), 
					startupHelper.ThumbnailStorage()).LoopFolder(
					fileIndexList, true);
			}

			Console.Write("^\n");
			Console.Write("~ Rename thumbnails ~");

			// Loop though all options
			fileIndexList.AddRange(toMetaFilesUpdate);

			// update thumbs to avoid unnecessary re-generation
			foreach ( var item in fileIndexList.GroupBy(i => i.FilePath).Select(g => g.First())
				.ToList() )
			{
				var newThumb = new FileHash(storage).GetHashCode(item.FilePath).Key;
				startupHelper.ThumbnailStorage().FileMove(item.FileHash, newThumb);
				if ( appSettings.Verbose )
					Console.WriteLine("thumb+ `" + item.FileHash + "`" + newThumb);
			}
		}
	}
}
