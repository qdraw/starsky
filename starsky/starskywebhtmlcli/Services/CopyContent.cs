using System;
using System.IO;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskywebhtmlcli.Services
{
	public class Content
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _istorage;

		public Content(AppSettings appSettings, IStorage storage)
		{
			_appSettings = appSettings;
			_istorage = storage;
		}

		public string GetContentFolder()
		{
			return AppDomain.CurrentDomain.BaseDirectory +
				   Path.DirectorySeparatorChar +
				   "Content";
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
