using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starsky.foundation.writemeta.Services;

public sealed class ExifCopy
{
	private const string XmpStartContent =
		"<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Starsky\'>" +
		"\n<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>\n" +
		"</rdf:RDF>\n</x:xmpmeta>";

	private readonly ExifToolCmdHelper _exifToolCmdHelper;
	private readonly IReadMeta _readMeta;
	private readonly IStorage _subPathStorage;

	public ExifCopy(IStorage subPathStorage, IStorage thumbnailStorage, IExifTool exifTool,
		IReadMeta readMeta, IThumbnailQuery thumbnailQuery, IWebLogger logger)
	{
		_subPathStorage = subPathStorage;
		_readMeta = readMeta;
		_exifToolCmdHelper = new ExifToolCmdHelper(exifTool, _subPathStorage,
			thumbnailStorage, _readMeta, thumbnailQuery, logger);
	}

	/// <summary>
	///     Create a simple xmp starter file
	/// </summary>
	/// <param name="xmpPath">location</param>
	public bool XmpCreate(string xmpPath)
	{
		if ( _subPathStorage.ExistFile(xmpPath) )
		{
			return false;
		}

		var plainTextStream = StringToStreamHelper.StringToStream(XmpStartContent);
		// dispose in WriteStream
		_subPathStorage.WriteStream(plainTextStream, xmpPath);
		return true;
	}

	/// <summary>
	///     Add a .xmp sidecar file
	/// </summary>
	/// <param name="subPath"></param>
	/// <returns></returns>
	public async Task<string> XmpSync(string subPath)
	{
		// only for raw files
		if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) )
		{
			return subPath;
		}

		var withXmpPath = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);

		// only for files that not exist yet
		if ( _subPathStorage.ExistFile(withXmpPath) )
		{
			return withXmpPath;
		}

		XmpCreate(withXmpPath);

		// Now copy content using exifTool
		await CopyExifPublish(subPath, withXmpPath);

		return withXmpPath;
	}

	/// <summary>
	///     Keep within the same storage provider
	///     Used for example by Import
	/// </summary>
	/// <param name="fromSubPath"></param>
	/// <param name="toSubPath"></param>
	/// <returns></returns>
	public async Task<string> CopyExifPublish(string fromSubPath, string toSubPath)
	{
		var updateModel = await _readMeta.ReadExifAndXmpFromFileAsync(fromSubPath);
		var comparedNames = FileIndexCompareHelper.Compare(new FileIndexItem(), updateModel);
		comparedNames.Add(nameof(FileIndexItem.Software));
		updateModel!.SetFilePath(toSubPath);
		return ( await _exifToolCmdHelper.UpdateAsync(updateModel,
			comparedNames, true, false) ).Command;
	}
}
