using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using starsky.feature.webhtmlpublish.Interfaces;
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
		private readonly IPublishPreflight _publishPreflight;

		private const string ManifestName = "_settings.json";

		public PublishManifest(IPublishPreflight publishPreflight, IStorage storage,  AppSettings appSettings, PlainTextFileHelper plainTextFileHelper)
		{
			_publishPreflight = publishPreflight;
			_storage = storage;
			_appSettings = appSettings;
			_plainTextFileHelper = plainTextFileHelper;
		}

		/// <summary>
		/// Export settings as manifest.json to the StorageFolder within appSettings
		/// </summary>
		/// <param name="fullFilePath"></param>
		/// <param name="itemName"></param>
		/// <param name="publishProfileName"></param>
		public void ExportManifest( string fullFilePath, string itemName, string publishProfileName)
		{
			
			//t o todo!!
			// missing appended items
			// _bigimages-helper.js
			
			var copy = _publishPreflight.GetPublishProfileName(publishProfileName).Select(p
				=> new Dictionary<string, bool>
				{
					{
						!string.IsNullOrEmpty(p.Folder)
							? p.Folder
							: p.Path.Replace(fullFilePath, string.Empty),
						p.Copy
					}
				});
			
			var manifest = new ManifestModel
			{
				Name = itemName,
				Copy = copy
			};
			var output = JsonConvert.SerializeObject(manifest, Formatting.Indented);
			var outputLocation = Path.Combine(fullFilePath, ManifestName);
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
			
			throw new Exception("_appSettings.Name is obsolete ");
			_appSettings.Name = manifestModel.Name;
			
			return true;
		}
	}
}
