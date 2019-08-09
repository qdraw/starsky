using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using starskycore.Models;

namespace starskycore.Helpers
{
	internal class ManifestModel
	{
		public string Name { get; set; }

		public string Slug
		{
			get { return new AppSettings().GenerateSlug(Name, true); }
		}

		public string Export => DateTime.Now.ToString("yyyyMMddHHmmss",CultureInfo.InvariantCulture);
		
		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString(); 

	}

	public class ExportManifest
	{
		private readonly AppSettings _appSettings;
		private readonly PlainTextFileHelper _plainTextFileHelper;

		private const string ManifestName = "_settings.json";

		public ExportManifest(AppSettings appSettings, PlainTextFileHelper plainTextFileHelper)
		{
			_appSettings = appSettings;
			_plainTextFileHelper = plainTextFileHelper;
		}

		/// <summary>
		/// Export settings as manifest.json to the StorageFolder within appsettings
		/// </summary>
		public void Export()
		{
			// Export settings as manifest.json to the StorageFolder
		
			var manifest = new ManifestModel
			{
				Name = _appSettings.Name,
			};
			var output = JsonConvert.SerializeObject(manifest);
			var outputLocation = Path.Combine(_appSettings.StorageFolder, ManifestName);
			FilesHelper.DeleteFile(outputLocation);
			_plainTextFileHelper.WriteFile(outputLocation, output);
		}

		/// <summary>
		/// Imports name to the appsettings. 
		/// false > file not exist
		/// </summary>
		/// <returns>false > file not exist</returns>
		public bool Import()
		{
			var fullSettingsPath = Path.Combine(_appSettings.StorageFolder, ManifestName);

			if ( FilesHelper.IsFolderOrFile(fullSettingsPath) !=
			     FolderOrFileModel.FolderOrFileTypeList.File ) return false;
			
			var input =_plainTextFileHelper.ReadFile(fullSettingsPath);

			var manifestModel = JsonConvert.DeserializeObject<ManifestModel>(input);
			_appSettings.Name = manifestModel.Name;
			
			return true;
		}
	}
}
