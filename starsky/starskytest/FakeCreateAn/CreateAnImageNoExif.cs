using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using starsky.foundation.platform.Helpers;
using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public class CreateAnImageNoExif
	{

		public readonly string FullFilePathWithDate = 
			Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) +
			Path.DirectorySeparatorChar + FileNameWithDate;

		private const string FileNameWithDate = "123300_20120101.jpg";
		// HHmmss_yyyyMMdd > not very logical but used to test features

		public readonly string FileName = FileNameWithDate;

		public readonly string BasePath =
			Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + 
			Path.DirectorySeparatorChar;

		[SuppressMessage("ReSharper", "StringLiteralTypo")] 
		private static readonly string Base64JpgString =	"/9j/4AAQSkZJRgABAQAAAQABAAD/2wDFAAEBAQEB"+
			"AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEAAQEBAQEBA" + 
			"QEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQABAQEBAQEBAQE" +
			"BAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/8EAEQgAAgADAwARA"+
			"AERAAIRAP/EACcAAQEAAAAAAAAAAAAAAAAAAAAKEAEAAAAAAAAAAAAAAAAAAAAA/9oADAMAAAEAAgAAPwC/gH//2Q==";

		public static readonly ImmutableArray<byte> Bytes = Base64Helper.TryParse(Base64JpgString).ToImmutableArray();

		public CreateAnImageNoExif()
		{

			if (!File.Exists(FullFilePathWithDate))
			{
				File.WriteAllBytes(FullFilePathWithDate, Convert.FromBase64String(Base64JpgString));
			}
		}
	}
}
