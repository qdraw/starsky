using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.sync.Helpers;

public class CheckForStatusNotOkHelper
{
	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;

	public CheckForStatusNotOkHelper(IStorage subPathStorage, IWebLogger logger)
	{
		_subPathStorage = subPathStorage;
		_logger = logger;
	}

	internal IEnumerable<FileIndexItem> CheckForStatusNotOk(IEnumerable<string> subPaths)
	{
		var result = new List<FileIndexItem>();
		foreach ( var subPath in subPaths )
		{
			result.AddRange(CheckForStatusNotOk(subPath));
		}

		return result;
	}

	/// <summary>
	///     When the file is not supported or does not exist return status
	/// </summary>
	/// <param name="subPath">relative path</param>
	/// <returns>item with status</returns>
	internal List<FileIndexItem> CheckForStatusNotOk(string subPath)
	{
		var statusItem = new FileIndexItem(subPath) { Status = FileIndexItem.ExifStatus.Ok };

		// File extension is not supported
		if ( !ExtensionRolesHelper.IsExtensionSyncSupported(subPath) )
		{
			statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			return new List<FileIndexItem> { statusItem };
		}

		if ( !_subPathStorage.ExistFile(subPath) )
		{
			statusItem.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
			return new List<FileIndexItem> { statusItem };
		}

		// File check if jpg #not corrupt
		var stream = _subPathStorage.ReadStream(subPath, 160);
		var imageFormat = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		stream.Dispose();

		// ReSharper disable once InvertIf
		if ( !ExtensionRolesHelper.ExtensionSyncSupportedList.Contains(imageFormat.ToString()) )
		{
			statusItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			return new List<FileIndexItem> { statusItem };
		}

		var xmpFilePath = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);
		if ( string.IsNullOrEmpty(xmpFilePath) ||
		     !_subPathStorage.ExistFile(xmpFilePath) ||
		     statusItem.FilePath == xmpFilePath )
		{
			return new List<FileIndexItem> { statusItem };
		}

		var xmpStatusItem = new FileIndexItem(xmpFilePath) { Status = FileIndexItem.ExifStatus.Ok };
		return new List<FileIndexItem> { statusItem, xmpStatusItem };
	}
}
