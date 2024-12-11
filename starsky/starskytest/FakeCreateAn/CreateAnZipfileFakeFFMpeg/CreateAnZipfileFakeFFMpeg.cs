using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnZipfileFakeFFMpeg;

public class CreateAnZipfileFakeFfMpeg
{
	public readonly ImmutableArray<byte> Bytes = [..Array.Empty<byte>()];

	public CreateAnZipfileFakeFfMpeg()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnZipfileFakeFFMpeg", "ffmpeg.zip");

		Bytes = [..StreamToBytes(path)];
	}

	public static List<string> Content { get; set; } = new() { "ffmpeg", "ffmpeg.exe" };

	private static byte[] StreamToBytes(string path)
	{
		var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
		using var ms = new MemoryStream();
		input.CopyTo(ms);
		input.Dispose();
		return ms.ToArray();
	}
}
