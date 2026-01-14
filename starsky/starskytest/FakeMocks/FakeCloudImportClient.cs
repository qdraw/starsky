using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.cloudimport;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks;

public sealed class FakeCloudImportClient : ICloudImportClient
{
	public List<CloudFile> FilesToReturn { get; set; } = new();
	public bool ShouldTestConnectionSucceed { get; set; } = true;
	public List<CloudFile> DeletedFiles { get; } = new();
	public Dictionary<string, string> DownloadedFiles { get; } = new();
	public string Name => "FakeProvider";
	public bool Enabled { get; set; } = true;
	public ManualResetEventSlim? SyncBlocker { get; set; }

	public Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
	{
		SyncBlocker?.Wait();
		return Task.FromResult(FilesToReturn);
	}

	public Task<string> DownloadFileAsync(CloudFile file, string localFolder)
	{
		var localPath = Path.Combine(localFolder, file.Name);
		DownloadedFiles[file.Name] = localPath;
		// Create a dummy file
		Directory.CreateDirectory(localFolder);
		File.WriteAllBytes(localPath, [.. CreateAnImage.Bytes]);
		return Task.FromResult(localPath);
	}

	public Task<bool> DeleteFileAsync(CloudFile file)
	{
		DeletedFiles.Add(file);
		return Task.FromResult(true);
	}

	public Task<bool> TestConnectionAsync()
	{
		return Task.FromResult(ShouldTestConnectionSucceed);
	}
}
