using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.ArchiveFormats.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIZipper : IZipper
{
	private readonly IStorage _storage;
	private readonly List<Tuple<string, byte[]>> _zipContent;

	public FakeIZipper(List<Tuple<string, byte[]>> zipContent, IStorage storage)
	{
		_storage = storage;
		_zipContent = zipContent;
	}

	public bool ExtractZip(string zipInputFullPath, string storeZipFolderFullPath)
	{
		var bytes = _zipContent.FirstOrDefault(p => p.Item1 == zipInputFullPath)?.Item2;
		if ( bytes == null )
		{
			Console.WriteLine("ExtractZip: " + zipInputFullPath + " not found");
			return false;
		}

		foreach ( var values in Zipper.ExtractZip(bytes) )
		{
			var outputPath = Path.Combine(storeZipFolderFullPath, values.Key);
			Console.WriteLine("ExtractZip: " + zipInputFullPath + " to " + outputPath);

			_storage.WriteStream(new MemoryStream(values.Value), outputPath);
		}

		return true;
	}
}
