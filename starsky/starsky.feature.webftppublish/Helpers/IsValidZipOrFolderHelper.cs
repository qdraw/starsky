using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.webftppublish.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.feature.webftppublish.Helpers;

/// <summary>
///     Helper to validate if input is a valid zip or folder with manifest
/// </summary>
public class IsValidZipOrFolderHelper
{
	private readonly IWebLogger _logger;
	private readonly IStorage _storage;

	public IsValidZipOrFolderHelper(IStorage storage, IWebLogger logger)
	{
		_storage = storage;
		_logger = logger;
	}

	/// <summary>
	///     Validates and extracts manifest from zip or folder
	/// </summary>
	/// <param name="inputFullFileDirectoryOrZip">Path to validate</param>
	/// <returns>Manifest or null</returns>
	public async Task<FtpPublishManifestModel?> IsValidZipOrFolder(
		string inputFullFileDirectoryOrZip)
	{
		if ( string.IsNullOrWhiteSpace(inputFullFileDirectoryOrZip) )
		{
			_logger.LogError("Please use the -p to add a path first");
			return null;
		}

		var inputPathType = _storage.IsFolderOrFile(inputFullFileDirectoryOrZip);

		switch ( inputPathType )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				_logger.LogError($"Folder location {inputFullFileDirectoryOrZip} " +
				                 $"is not found \nPlease try the `-h` command to get help ");
				return null;
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var settingsFullFilePath =
					Path.Combine(inputFullFileDirectoryOrZip, "_settings.json");
				if ( _storage.ExistFile(settingsFullFilePath) )
				{
					return await
						new DeserializeJson(_storage).ReadAsync<FtpPublishManifestModel>(
							settingsFullFilePath);
				}

				_logger.LogError("Please run 'starskywebhtmlcli' " +
				                 "first to generate a settings file");
				return null;
			}
			case FolderOrFileModel.FolderOrFileTypeList.File:
				if ( !string.Equals(Path.GetExtension(inputFullFileDirectoryOrZip), ".zip",
					    StringComparison.OrdinalIgnoreCase) )
				{
					return null;
				}

				var zipFirstByteStream = _storage.ReadStream(inputFullFileDirectoryOrZip, 10);
				if ( !Zipper.IsValidZipFile(zipFirstByteStream) )
				{
					_logger.LogError(
						$"Zip file is invalid or unreadable {inputFullFileDirectoryOrZip}");
					return null;
				}

				var manifest =
					new Zipper(_logger).ExtractZipEntry(inputFullFileDirectoryOrZip,
						"_settings.json");
				if ( manifest == null )
				{
					return null;
				}

				var result = JsonSerializer.Deserialize<FtpPublishManifestModel>(manifest);
				return result;
			default:
				return null;
		}
	}
}
