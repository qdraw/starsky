using System;
using System.IO;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class Content
	{
		private readonly IStorage _iStorage;

		public Content(IStorage storage)
		{
			_iStorage = storage;
		}

		private string GetContentFolder()
		{
			return AppDomain.CurrentDomain.BaseDirectory +
				   Path.DirectorySeparatorChar +
				   "WebHtmlPublish" +
				   Path.DirectorySeparatorChar +
				   "PublishedContent";
		}

		public void CopyPublishedContent()
		{
			
			// todo REFACTOR!!
			
			var files = new StorageHostFullPathFilesystem().GetAllFilesInDirectory(GetContentFolder());
			foreach ( var file in files)
			{
				var input = new StorageHostFullPathFilesystem().ReadStream(file);
				_iStorage.WriteStream(input, Path.GetFileName(file));
			}
		}

	}
}
