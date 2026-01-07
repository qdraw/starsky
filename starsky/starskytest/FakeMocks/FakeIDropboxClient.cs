using System;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using starsky.feature.cloudimport.Clients.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIDropboxClient : IDropboxClient
{
	public FakeIDropboxClient(FakeFilesUserRoutes files)
	{
		Files = files;
	}

	public FakeFilesUserRoutes Files { get; set; }
	public bool Disposed { get; private set; }

	public void Dispose()
	{
		Disposed = true;
	}

	public Task<ListFolderResult> ListFolderAsync(string path)
	{
		return Files.ListFolderAsync(path);
	}

	public Task<ListFolderResult> ListFolderContinueAsync(string arg)
	{
		throw new NotImplementedException();
	}

	public Task<IDownloadResponse<FileMetadata>> DownloadAsync(string path, string? rev = null)
	{
		throw new NotImplementedException();
	}

	public Task<DeleteResult> DeleteV2Async(string path, string? parentRev = null)
	{
		throw new NotImplementedException();
	}
}
