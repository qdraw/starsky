using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.cloudimport;

namespace starskytest.FakeMocks;

public class FakeCloudImportClientWithException : ICloudImportClient
{
	public string Name => "FakeProvider";
	public bool Enabled => true;
	public Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
	{
		throw new Exception("Simulated ListFilesAsync failure");
	}
	public Task<string> DownloadFileAsync(CloudFile file, string localFolder) => Task.FromResult("");
	public Task<bool> DeleteFileAsync(CloudFile file) => Task.FromResult(true);
	public Task<bool> TestConnectionAsync() => Task.FromResult(true);
}

