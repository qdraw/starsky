namespace starsky.foundation.cloudimport;

public interface ICloudImportClient
{
	string Name { get; }
	bool Enabled { get; }
	Task<List<CloudFile>> ListFilesAsync(string remoteFolder);
	Task<string> DownloadFileAsync(CloudFile file, string localFolder);
	Task<bool> DeleteFileAsync(CloudFile file);
	Task<bool> TestConnectionAsync();
}
