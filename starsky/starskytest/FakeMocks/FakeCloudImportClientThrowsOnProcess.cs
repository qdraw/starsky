using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.cloudimport;

namespace starskytest.FakeMocks;

public class FakeCloudImportClientThrowsOnProcess : ICloudImportClient
{
	public string Name => "FakeProvider";
	public bool Enabled => true;

	public Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
	{
		return Task.FromResult(new List<CloudFile>());
	}

	public Task<string> DownloadFileAsync(CloudFile file, string localFolder)
	{
		throw new Exception("Download failed");
	}

	public Task<bool> DeleteFileAsync(CloudFile file)
	{
		return Task.FromResult(true);
	}

	public Task<bool> TestConnectionAsync()
	{
		return Task.FromResult(true);
	}
}
