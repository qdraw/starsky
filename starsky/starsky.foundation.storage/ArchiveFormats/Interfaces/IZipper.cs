namespace starsky.foundation.storage.ArchiveFormats.Interfaces;

public interface IZipper
{
	bool ExtractZip(string zipInputFullPath, string storeZipFolderFullPath);
}
