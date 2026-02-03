using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using starsky.feature.webhtmlpublish.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers;

public class PublishManifest
{
	private const string ManifestName = "_settings.json";
	private readonly IStorage _storage;

	public PublishManifest(IStorage storage)
	{
		_storage = storage;
	}

	/// <summary>
	///     Export settings as manifest.json to the StorageFolder within appSettings
	/// </summary>
	/// <param name="parentFullFilePath">without ManifestName</param>
	/// <param name="itemName"></param>
	/// <param name="copyContent"></param>
	public PublishManifestModel ExportManifest(string parentFullFilePath, string itemName,
		Dictionary<string, bool>? copyContent)
	{
		copyContent ??= new Dictionary<string, bool>();
		var manifest = new PublishManifestModel { Name = itemName, Copy = copyContent };
		var output = JsonSerializer.Serialize(manifest, DefaultJsonSerializer.NoNamingPolicy);
		var outputLocation = Path.Combine(parentFullFilePath, ManifestName);

		_storage.FileDelete(outputLocation);
		_storage.WriteStream(StringToStreamHelper.StringToStream(output), outputLocation);

		return manifest;
	}

	// /// <summary>
	// ///     Export FTP publish settings with profile name to _settings.json
	// /// </summary>
	// /// <param name="parentFullFilePath">without ManifestName</param>
	// /// <param name="itemName">item name</param>
	// /// <param name="copyContent">files to copy</param>
	// /// <param name="publishProfileName">the publish profile name being used</param>
	// public PublishManifestModel ExportFtpManifest(string parentFullFilePath, string itemName,
	// 	Dictionary<string, bool>? copyContent, string? publishProfileName = null)
	// {
	// 	copyContent ??= new Dictionary<string, bool>();
	// 	var manifest = new PublishManifestModel
	// 	{
	// 		Slug = itemName, Copy = copyContent, 
	// 		= publishProfileName
	// 	};
	// 	var output = JsonSerializer.Serialize(manifest, DefaultJsonSerializer.NoNamingPolicy);
	// 	var outputLocation = Path.Combine(parentFullFilePath, ManifestName);
	//
	// 	_storage.FileDelete(outputLocation);
	// 	_storage.WriteStream(StringToStreamHelper.StringToStream(output), outputLocation);
	//
	// 	return manifest;
	// }
}
