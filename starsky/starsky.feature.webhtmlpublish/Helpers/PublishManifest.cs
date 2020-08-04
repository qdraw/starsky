using System.IO;
using Newtonsoft.Json;
using starsky.feature.webhtmlpublish.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class PublishManifest
	{
		private readonly AppSettings _appSettings;
		private readonly PlainTextFileHelper _plainTextFileHelper;
		private readonly IStorage _storage;

		private const string ManifestName = "_settings.json";

		public PublishManifest(IStorage storage,  AppSettings appSettings, PlainTextFileHelper plainTextFileHelper)
		{
			_storage = storage;
			_appSettings = appSettings;
			_plainTextFileHelper = plainTextFileHelper;
		}

		/// <summary>
		/// Export settings as manifest.json to the StorageFolder within appsettings
		/// </summary>
		public void ExportManifest()
		{
			// Export settings as manifest.json to the StorageFolder
		
			var manifest = new ManifestModel
			{
				Name = _appSettings.Name,
			};
			var output = JsonConvert.SerializeObject(manifest);
			var outputLocation = Path.Combine(_appSettings.StorageFolder, ManifestName);
			_storage.FileDelete(outputLocation);

			_storage.WriteStream(_plainTextFileHelper.StringToStream(output), outputLocation);
		}

		/// <summary>
		/// Imports name to the appSettings. 
		/// false > file not exist
		/// </summary>
		/// <returns>false > file not exist</returns>
		public bool ImportManifest()
		{
			var fullSettingsPath = Path.Combine(_appSettings.StorageFolder, ManifestName);

			if ( !_storage.ExistFile(fullSettingsPath) ) return false;
			
			var input = _plainTextFileHelper.StreamToString(_storage.ReadStream(fullSettingsPath));
			var manifestModel = JsonConvert.DeserializeObject<ManifestModel>(input);
			_appSettings.Name = manifestModel.Name;
			
			return true;
		}
	}
}
