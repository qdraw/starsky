using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using starskycore.Models;
using starskycore.Services;

namespace starskycore.Helpers
{
	internal class ManifestModel
	{
		public string Name { get; set; }

		public string Slug
		{
			get
			{
				var slug = new AppSettings().GetWebSafeReplacedName(Name);
				return ConfigRead.RemoveLatestSlash(slug);
			}
		}

		public string Export => DateTime.Now.ToString("yyyyMMddHHmmss",CultureInfo.InvariantCulture);
	}

	public class ExportManifest
	{
		private readonly AppSettings _appSettings;
		private readonly PlainTextFileHelper _plainTextFileHelper;


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
			var outputLocation = Path.Combine(_appSettings.StorageFolder, "manifest.json");
			Files.DeleteFile(outputLocation);
			_plainTextFileHelper.WriteFile(outputLocation, output);
		}

		/// <summary>
		/// Imports name to the appsettings.
		/// </summary>
		public void Import()
		{
			var input =_plainTextFileHelper.ReadFile(Path.Combine(_appSettings.StorageFolder, "manifest.json"));

			var manifestModel = JsonConvert.DeserializeObject<ManifestModel>(input);
			_appSettings.Name = manifestModel.Name;
		}
	}
}
