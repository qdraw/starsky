namespace starsky.foundation.cloudsync;

public interface ICloudSyncClient
{
	string Name { get; }
	bool Enabled { get; }
	Task<IEnumerable<CloudFile>> ListFilesAsync(string remoteFolder);
	Task<string> DownloadFileAsync(CloudFile file, string localFolder);
}	
