using System;
using System.IO;
using starskycore.Interfaces;
using starskycore.Services;
using starskycore.Storage;

namespace starskywebhtmlcli.Services
{
	public class Content
	{
		private readonly IStorage _istorage;

		public Content(IStorage storage)
		{
			_istorage = storage;
		}

		private string GetContentFolder()
		{
			return AppDomain.CurrentDomain.BaseDirectory +
				   Path.DirectorySeparatorChar +
				   "PublishedContent";
		}

		public void CopyContent()
		{
			var files = new StorageHostFullPathFilesystem().GetAllFilesInDirectory(GetContentFolder());
			foreach ( var file in files)
			{
				var input = new StorageHostFullPathFilesystem().ReadStream(file);
				_istorage.WriteStream(input, Path.GetFileName(file));
			}
		}

	}
}
