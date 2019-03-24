using System;
using System.IO;
using System.Reflection;
using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public class CreateAnImageNoExif
	{

		public readonly string FullFilePathWithDate = 
			Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + FileNameWithDate;

		private const string FileNameWithDate = "123300_20120101.jpg";
		// HHmmss_yyyyMMdd > not very logical but used to test features

		public readonly string FileName = FileNameWithDate;

		public readonly string BasePath =
		Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

		private static readonly string Base64JpgString =	"/9j/4AAQSkZJRgABAQAAAQABAAD/2wDFAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEAAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/8EAEQgAAgADAwARAAERAAIRAP/EACcAAQEAAAAAAAAAAAAAAAAAAAAKEAEAAAAAAAAAAAAAAAAAAAAA/9oADAMAAAEAAgAAPwC/gH//2Q==";

		public static readonly byte[] Bytes = Base64Helper.TryParse(Base64JpgString);
		
		public CreateAnImageNoExif()
		{

			if (!File.Exists(FullFilePathWithDate))
			{
			File.WriteAllBytes(FullFilePathWithDate, Convert.FromBase64String(Base64JpgString));
			}
		}
	}
}
