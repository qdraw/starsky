using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text.Json;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.FakeCreateAn.CreateAnImageLongDescriptionTitle;

public class CreateAnImageLongDescriptionTitle
{
	public readonly ImmutableArray<byte> Bytes = [..Array.Empty<byte>()];

	public CreateAnImageLongDescriptionTitle()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageLongDescriptionTitle", "CreateAnImageLongDescriptionTitle.jpg");

		Bytes = [..StreamToBytes(path)];
		ReadAndSetJson(dirName);
	}

	public CreateAnImageLongDescriptionTitleExampleDataModel? JsonExpectData { get; set; }

	private void ReadAndSetJson(string dirName)
	{
		var jsonPath = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnImageLongDescriptionTitle", "CreateAnImageLongDescriptionTitle.json");
		var json = File.ReadAllText(jsonPath);
		JsonExpectData =
			JsonSerializer.Deserialize<CreateAnImageLongDescriptionTitleExampleDataModel>(json);
	}

	private static byte[] StreamToBytes(string path)
	{
		var input = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
		using var ms = new MemoryStream();
		input.CopyTo(ms);
		input.Dispose();
		return ms.ToArray();
	}
}
