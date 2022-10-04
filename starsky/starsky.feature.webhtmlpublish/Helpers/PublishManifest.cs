using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using starsky.feature.webhtmlpublish.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class PublishManifest
	{
		private readonly IStorage _storage;

		private const string ManifestName = "_settings.json";

		public PublishManifest(IStorage storage)
		{
			_storage = storage;
		}

		/// <summary>
		/// Export settings as manifest.json to the StorageFolder within appSettings
		/// </summary>
		/// <param name="parentFullFilePath">without ManifestName</param>
		/// <param name="itemName"></param>
		/// <param name="copyContent"></param>
		public void ExportManifest( string parentFullFilePath, string itemName, 
			Dictionary<string, bool> copyContent)
		{
			var manifest = new PublishManifestModel
			{
				Name = itemName,
				Copy = copyContent
			};
			var output = JsonSerializer.Serialize(manifest,
				new JsonSerializerOptions
				{
					DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
					WriteIndented = true
				});
			var outputLocation = Path.Combine(parentFullFilePath, ManifestName);
			
			_storage.FileDelete(outputLocation);
			_storage.WriteStream(PlainTextFileHelper.StringToStream(output), outputLocation);
		}
	}
}
