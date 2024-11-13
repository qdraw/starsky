using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;

namespace starsky.foundation.thumbnailmeta.Helpers;

public class AddMetaPreviewDirectoryHelper(
	IStorage storage,
	IWebLogger logger,
	AppSettings appSettings)
{
	public delegate Task<(bool, bool, string, string?)> AddMetaThumbnailDelegate(string subPath,
		string fileHash);

	public async Task<List<(bool, bool, string, string?)>> AddMetaPreviewDirectory(
		AddMetaThumbnailDelegate @delegate, string subPath)
	{
		var isFolderOrFile = storage.IsFolderOrFile(subPath);
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch ( isFolderOrFile )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				logger.LogError($"[AddMetaThumbnail] folder or file not found {subPath}");
				return new List<(bool, bool, string, string?)>
				{
					( false, false, subPath, "folder or file not found" )
				};
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var contentOfDir = storage.GetAllFilesInDirectoryRecursive(subPath)
					.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();

				var results = await contentOfDir
					.ForEachAsync(async singleSubPath =>
							await @delegate.Invoke(singleSubPath, null!),
						appSettings.MaxDegreesOfParallelism);

				return results!.ToList();
			}
			default:
			{
				var result = await new FileHash(storage).GetHashCodeAsync(subPath);
				return !result.Value
					? new List<(bool, bool, string, string?)>
					{
						( false, false, subPath, "hash not found" )
					}
					: new List<(bool, bool, string, string?)>
					{
						await @delegate(subPath, result.Key)
					};
			}
		}
	}
}
