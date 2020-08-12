using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using starsky.feature.webhtmlpublish.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class PublishManifest
	{
		private readonly PlainTextFileHelper _plainTextFileHelper;
		private readonly IStorage _storage;

		private const string ManifestName = "_settings.json";

		public PublishManifest(IStorage storage, PlainTextFileHelper plainTextFileHelper)
		{
			_storage = storage;
			_plainTextFileHelper = plainTextFileHelper;
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
			var output = JsonConvert.SerializeObject(manifest, Formatting.Indented);
			var outputLocation = Path.Combine(parentFullFilePath, ManifestName);
			
			_storage.FileDelete(outputLocation);
			_storage.WriteStream(_plainTextFileHelper.StringToStream(output), outputLocation);
		}
	}
}
