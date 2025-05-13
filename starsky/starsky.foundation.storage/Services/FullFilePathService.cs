using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.Services;

[Service(typeof(IFullFilePathService), InjectionLifetime = InjectionLifetime.Scoped)]
public class FullFilePathService(ISelectorStorage selectorStorage, AppSettings appSettings)
	: IFullFilePathService
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage _tempStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

	/// <summary>
	///     Get a full file path, if needed copy it to a temp folder
	/// </summary>
	/// <param name="subPath">subPath style</param>
	/// <returns>(fullFilePath, isTempFile, fileHashWithExtension)</returns>
	public async Task<(string, bool, string)> GetFullFilePath(string subPath,
		string beforeFileHash)
	{
		var fullFilePath = appSettings.DatabasePathToFilePath(subPath);

		if ( _hostStorage.ExistFile(appSettings.DatabasePathToFilePath(subPath)) )
		{
			return ( fullFilePath, false, string.Empty );
		}

		// Copy to Temp
		var sourceStream = _storage.ReadStream(subPath);
		if ( sourceStream == Stream.Null )
		{
			return ( fullFilePath, false, string.Empty );
		}

		var fileHashWithExtension = GetTempFileHashWithExtension(subPath, beforeFileHash);
		await _tempStorage.WriteStreamAsync(sourceStream, fileHashWithExtension);
		fullFilePath = appSettings.DatabasePathToTempFolderFilePath(fileHashWithExtension);

		return ( fullFilePath, true, fileHashWithExtension );
	}

	private static string GetTempFileHashWithExtension(string subPath, string beforeFileHash)
	{
		var fileHashWithExtension =
			$"{beforeFileHash}.{FilenamesHelper.GetFileExtensionWithoutDot(subPath)}";
		return fileHashWithExtension;
	}
}
