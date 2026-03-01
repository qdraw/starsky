using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using starsky.feature.webftppublish.Helpers;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webftppublish.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers.Slug;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.webftppublish.Services;

[Service(typeof(ILocalFileSystemPublishService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class LocalFileSystemPublishService(
	AppSettings appSettings,
	ISelectorStorage selectorStorage,
	IConsole console,
	IWebLogger logger)
	: ILocalFileSystemPublishService
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);


	/// <summary>
	///     Copy all content to the local file system destination
	/// </summary>
	/// <param name="parentDirectoryOrZipFile">Source directory or zip</param>
	/// <param name="profileId">Profile ID</param>
	/// <param name="slug">Slug/name</param>
	/// <param name="copyContent">Files to copy</param>
	/// <returns>True on success</returns>
	public bool Run(string parentDirectoryOrZipFile, string profileId, string slug,
		Dictionary<string, bool> copyContent)
	{
		var resultModel =
			new ExtractZipHelper(_hostStorage, logger).ExtractZip(parentDirectoryOrZipFile);
		LastExtractZipResult = resultModel;
		if ( resultModel.IsError )
		{
			return false;
		}

		var settings =
			appSettings.PublishProfilesRemote.GetLocalFileSystemById(profileId);

		if ( settings.Count == 0 )
		{
			logger.LogError($"No local file system settings found for profile: {profileId}");
			return false;
		}

		foreach ( var setting in settings )
		{
			if ( !CopyToLocalFileSystem(setting, resultModel.FullFileFolderPath, slug,
				    copyContent) )
			{
				return false;
			}

			if ( resultModel.RemoveFolderAfterwards )
			{
				_hostStorage.FolderDelete(resultModel.FullFileFolderPath);
			}

			console.Write("\n");
		}

		return true;
	}

	/// <summary>
	///     Copy files to local file system destination
	/// </summary>
	/// <param name="setting">Local file system credential</param>
	/// <param name="sourceDirectory">Source directory</param>
	/// <param name="slug">Slug/name for destination subdirectory</param>
	/// <param name="copyContent">Files to copy</param>
	/// <returns>True on success</returns>
	internal bool CopyToLocalFileSystem(LocalFileSystemCredential setting,
		string sourceDirectory, string slug, Dictionary<string, bool> copyContent)
	{
		var destinationBasePath = Path.Combine(setting.Path,
			GenerateSlugHelper.GenerateSlug(slug, true));

		var destinationStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

		// Create base destination directory
		if ( !destinationStorage.ExistFolder(destinationBasePath) )
		{
			console.Write(",");
			destinationStorage.CreateDirectory(destinationBasePath);
		}

		// Create subdirectories
		foreach ( var copyItem in copyContent.Where(p => p.Value) )
		{
			var parentItems = Breadcrumbs.BreadcrumbHelper(copyItem.Key);
			var validItems = parentItems
				.Where(p => p != Path.DirectorySeparatorChar.ToString())
				.Where(item => _hostStorage.ExistFolder(sourceDirectory + item));

			foreach ( var item in validItems )
			{
				var destDir = Path.Combine(destinationBasePath, item.TrimStart('/'));
				if ( destinationStorage.ExistFolder(destDir) )
				{
					continue;
				}

				console.Write(",");
				destinationStorage.CreateDirectory(destDir);
			}
		}

		// Copy files
		var filesToCopy = copyContent.Where(p => p.Value).Select(p => p.Key).ToList();
		foreach ( var fileSubPath in filesToCopy )
		{
			var sourcePath = Path.Combine(sourceDirectory, fileSubPath.TrimStart('/'));
			var destPath = Path.Combine(destinationBasePath, fileSubPath.TrimStart('/'));

			if ( !_hostStorage.ExistFile(sourcePath) )
			{
				console.WriteLine($"Fail > source file not found => {sourcePath}");
				return false;
			}

			console.Write(".");

			try
			{
				using var sourceStream = _hostStorage.ReadStream(sourcePath);
				destinationStorage.WriteStream(sourceStream, destPath);
			}
			catch ( Exception ex )
			{
				logger.LogError(ex, $"Failed to copy file {sourcePath} to {destPath}");
				console.WriteLine($"Fail > copy file => {fileSubPath}");
				return false;
			}
		}

		return true;
	}

	// Expose for testing
	internal ExtractZipResultModel? LastExtractZipResult { get; private set; }
}
