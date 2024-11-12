using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.thumbnailmeta.Helpers;

public class PreflightCheck
{
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;

	public PreflightCheck(IStorage storage, IWebLogger logger)
	{
		_iStorage = storage;
		_logger = logger;
	}

	public async Task<string> GetFileHash(string subPath, string fileHash)
	{
		if ( !string.IsNullOrEmpty(fileHash) )
		{
			return fileHash;
		}

		var result = await new FileHash(_iStorage).GetHashCodeAsync(subPath);
		if ( !result.Value )
		{
			_logger.LogError("[MetaExifThumbnail] hash failed");
			return string.Empty;
		}

		fileHash = result.Key;
		return fileHash;
	}

	/// <summary>
	/// </summary>
	/// <param name="subPath"></param>
	/// <returns>fail/pass, right type, string=subPath, string?2= error reason</returns>
	public ( bool, bool, string, string ) Check(string subPath)
	{
		if ( !_iStorage.ExistFile(subPath) )
		{
			return ( false, false, subPath, "not found" );
		}

		var first50BytesStream = _iStorage.ReadStream(subPath, 50);
		var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);

		if ( imageFormat != ExtensionRolesHelper.ImageFormat.jpg &&
		     imageFormat != ExtensionRolesHelper.ImageFormat.tiff )
		{
			_logger.LogDebug($"[AddMetaThumbnail] {subPath} is not a jpg or tiff file");
			return ( false, false, subPath, $"{subPath} is not a jpg or tiff file" );
		}

		return ( true, true, string.Empty, string.Empty );
	}
}
