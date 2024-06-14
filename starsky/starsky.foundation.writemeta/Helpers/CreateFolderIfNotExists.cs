using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.writemeta.Helpers;

public class CreateFolderIfNotExists
{
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly AppSettings _appSettings;

	public CreateFolderIfNotExists(IWebLogger logger, AppSettings appSettings)
	{
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(logger);
		_logger = logger;
		_appSettings = appSettings;
	}

	internal void CreateDirectoryDependenciesTempFolderIfNotExists()
	{
		CreateDirectoryDependenciesFolderIfNotExists();
		CreateDirectoryTempFolderIfNotExists();
	}

	private void CreateDirectoryDependenciesFolderIfNotExists()
	{
		if ( _hostFileSystemStorage.ExistFolder(_appSettings
			    .DependenciesFolder) )
		{
			return;
		}

		_logger.LogInformation("[DownloadExifTool] Create Directory: " +
		                       _appSettings.DependenciesFolder);
		_hostFileSystemStorage.CreateDirectory(_appSettings.DependenciesFolder);
	}

	private void CreateDirectoryTempFolderIfNotExists()
	{
		if ( _hostFileSystemStorage.ExistFolder(_appSettings
			    .TempFolder) )
		{
			return;
		}

		_logger.LogInformation("[DownloadExifTool] Create Directory: " +
		                       _appSettings.TempFolder);
		_hostFileSystemStorage.CreateDirectory(_appSettings.TempFolder);
	}
}
