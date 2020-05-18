using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public class CreateAnExifToolWindows
	{
		public CreateAnExifToolWindows()
		{
			// In the project you need to include this:
			
			// <ItemGroup>
			// 	<None Remove="FakeCreateAn\CreateFakeExifToolWindows\exiftool.zip" />
			// 	<Content Include="FakeCreateAn\CreateFakeExifToolWindows\exiftool.zip">
			// 	<CopyToOutputDirectory>Always</CopyToOutputDirectory>
			// 	</Content>
			// 	</ItemGroup>
			
			var baseDirectoryProject = AppDomain.CurrentDomain.BaseDirectory;

			var hostFullPathFilesystem = new StorageHostFullPathFilesystem();
			var exifToolWindowsZip = Path.Combine(baseDirectoryProject, "FakeCreateAn",
				"CreateFakeExifToolWindows", "exiftool.zip");
			
			if ( !hostFullPathFilesystem.ExistFile(exifToolWindowsZip) )
			{
				throw new FileNotFoundException("Do include windows ExifTool dummy please");
			}

			using var stream = hostFullPathFilesystem.ReadStream(exifToolWindowsZip);
			var streamWindows = new MemoryStream();
			stream.CopyTo(streamWindows);
			StreamByteArray = streamWindows.ToArray();
			streamWindows.Dispose();
		}

		public byte[] StreamByteArray { get; set; }

	}
}
