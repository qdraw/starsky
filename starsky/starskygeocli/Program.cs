using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskyGeoCli.Services;

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
			       && new ArgsHelper(appSettings).GetSubpathRelative(args).Length <= 1 ) )
			{
				appSettings.ApplicationType = AppSettings.StarskyAppType.Geo;
				new ArgsHelper(appSettings).NeedHelpShowDialog();
				return;
			}

			// Using both options
			string inputPath;
			// -s = ifsubpath || -p is path
			if ( new ArgsHelper(appSettings).IfSubpathOrPath(args) )
			{
				inputPath = appSettings.DatabasePathToFilePath(
					new ArgsHelper(appSettings).GetSubpathFormArgs(args)
				);
			}
			else
			{
				inputPath = new ArgsHelper(appSettings).GetPathFormArgs(args, false);
			}

			// overwrite subpath with relative days
			// use -g or --SubpathRelative to use it.
			// envs are not supported
			var getSubPathRelative = new ArgsHelper(appSettings).GetSubpathRelative(args);
			if ( getSubPathRelative != string.Empty )
			{
				inputPath = appSettings.DatabasePathToFilePath(getSubPathRelative);
			}

			// used in this session to find the files back
			appSettings.StorageFolder = inputPath;
			var storage = new StorageSubPathFilesystem(appSettings);

			if ( storage.IsFolderOrFile("/") == FolderOrFileModel.FolderOrFileTypeList.Deleted )
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
					new GeoIndexGpx(appSettings, startupHelper.ReadMeta(), storage).LoopFolder(
						fileIndexList);
				Console.Write("¬");
				new GeoLocationWrite(appSettings, startupHelper.ExifTool()).LoopFolder(
					toMetaFilesUpdate, false);
				Console.Write("(gps added)");
			}

			fileIndexList =
				new GeoReverseLookup(appSettings).LoopFolderLookup(fileIndexList,
					new ArgsHelper().GetAll(args));
			if ( fileIndexList.Count >= 1 )
			{
				Console.Write("~ Add city, state and country info ~");
				new GeoLocationWrite(appSettings, startupHelper.ExifTool()).LoopFolder(
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
				var newThumb = new FileHash(storage).GetHashCode(item.FilePath);
				storage.ThumbnailMove(item.FileHash, newThumb);
				if ( appSettings.Verbose )
					Console.WriteLine("thumb+ `" + item.FileHash + "`" + newThumb);
			}
		}
	}
}
