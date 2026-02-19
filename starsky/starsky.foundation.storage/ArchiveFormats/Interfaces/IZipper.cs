using System.Collections.Generic;

namespace starsky.foundation.storage.ArchiveFormats.Interfaces;

public interface IZipper
{
	Dictionary<string, byte[]> ExtractZip(byte[] zipped);
	bool ExtractZip(string zipInputFullPath, string storeZipFolderFullPath);
	byte[]? ExtractZipEntry(string zipInputFullPath, string entryFullName);
}
