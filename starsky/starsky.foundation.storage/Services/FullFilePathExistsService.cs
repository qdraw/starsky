using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.Services;

[Service(typeof(IFullFilePathExistsService), InjectionLifetime = InjectionLifetime.Scoped)]
public class FullFilePathExistsService(
	ISelectorStorage selectorStorage,
	AppSettings appSettings,
	ITenantSettings? tenantSettings = null)
	: IFullFilePathExistsService
{
	// the folders of the tenant of this request, not the (default) process wide ones
	private readonly ITenantSettings _tenantSettings =
		tenantSettings ?? new TenantSettings(TenantIdHelper.DefaultTenantId, appSettings);

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
	public async Task<FullFilePathExistsResultModel> GetFullFilePath(string subPath,
		string beforeFileHashWithoutExtension)
	{
		var fullFilePath = _tenantSettings.DatabasePathToFilePath(subPath);

		if ( _hostStorage.ExistFile(_tenantSettings.DatabasePathToFilePath(subPath)) )
		{
			// Exists on host machine
			return new FullFilePathExistsResultModel(
				true, fullFilePath, false, string.Empty);
		}

		var sourceStream = _storage.ReadStream(subPath);
		if ( sourceStream == Stream.Null )
		{
			// non existing file
			return new FullFilePathExistsResultModel(
				false, fullFilePath, false,
				string.Empty);
		}

		// temp file
		var tempFileHashWithExtension =
			GetTempFileHashWithExtension(subPath, beforeFileHashWithoutExtension);
		await _tempStorage.WriteStreamAsync(sourceStream, tempFileHashWithExtension);
		fullFilePath = _tenantSettings.DatabasePathToTempFolderFilePath(tempFileHashWithExtension);

		return new FullFilePathExistsResultModel(
			true, fullFilePath, true, tempFileHashWithExtension);
	}

	public void CleanTemporaryFile(string fileHashWithExtension, bool useTempStorageForInput)
	{
		if ( !useTempStorageForInput )
		{
			return;
		}

		_tempStorage.FileDelete(fileHashWithExtension);
	}

	private static string GetTempFileHashWithExtension(string subPath, string beforeFileHash)
	{
		var fileHashWithExtension =
			$"{beforeFileHash}.{FilenamesHelper.GetFileExtensionWithoutDot(subPath)}";
		return fileHashWithExtension;
	}
}
