using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class CopyPublishedContent
	{
		private readonly IStorage _hostStorage;
		private readonly ToCreateSubfolder _toCreateSubfolder;
		private  readonly AppSettings _appSettings;

		public CopyPublishedContent(AppSettings appSettings, ToCreateSubfolder toCreateSubfolder,
			ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_toCreateSubfolder = toCreateSubfolder;
			if ( selectorStorage == null ) return;
			_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}
		
		public Dictionary<string, bool> CopyContent(
			AppSettingsPublishProfiles profile,
			string outputParentFullFilePathFolder)
		{
			_toCreateSubfolder.Create(profile, outputParentFullFilePathFolder);
			var parentFolder = PathHelper.AddBackslash(
				_appSettings.GenerateSlug(profile.Folder, true));

			var copyResult = new Dictionary<string, bool>();
			var files = _hostStorage.GetAllFilesInDirectory(GetContentFolder()).ToList();
			foreach ( var file in files)
			{
				var subPath = parentFolder + Path.GetFileName(file);
				copyResult.Add(subPath, true);
				var fillFileOutputPath = Path.Combine(outputParentFullFilePathFolder, subPath);
				if ( !_hostStorage.ExistFile(fillFileOutputPath) )
				{
					_hostStorage.FileCopy(file,fillFileOutputPath);
				}
			}
			return copyResult;
		}

		internal string GetContentFolder()
		{
			return PathHelper.RemoveLatestBackslash(AppDomain.CurrentDomain.BaseDirectory)  +
			       Path.DirectorySeparatorChar +
			       "WebHtmlPublish" +
			       Path.DirectorySeparatorChar +
			       "PublishedContent";
		}
	}
}
