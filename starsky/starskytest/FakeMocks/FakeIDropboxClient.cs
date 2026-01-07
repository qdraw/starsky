using System;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using starsky.feature.cloudimport.Clients.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIDropboxClient : IDropboxClient
{
	private bool _disposed;

	public FakeIDropboxClient(FakeFilesUserRoutes files)
	{
		Files = files;
	}

	public FakeFilesUserRoutes Files { get; set; }

	public Func<string, Task<ListFolderResult>>? ListFolderContinueAsyncFunc { get; set; }

	public Func<string, Task<IDownloadResponse<FileMetadata>>>? DownloadAsyncFunc { get; set; }

	public Func<string, Task<DeleteResult>>? DeleteV2AsyncFunc { get; set; }

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public Task<ListFolderResult> ListFolderAsync(string path)
	{
		return Files.ListFolderAsync(path);
	}

	public Task<ListFolderResult> ListFolderContinueAsync(string arg)
	{
		return ListFolderContinueAsyncFunc != null
			? ListFolderContinueAsyncFunc(arg)
			: throw new NotImplementedException();
	}

	public Task<IDownloadResponse<FileMetadata>> DownloadAsync(string path, string? rev = null)
	{
		if ( DownloadAsyncFunc != null )
		{
			return DownloadAsyncFunc(path);
		}

		throw new NotImplementedException();
	}

	public Task<DeleteResult> DeleteV2Async(string path, string? parentRev = null)
	{
		if (DeleteV2AsyncFunc != null)
		{
			return DeleteV2AsyncFunc(path);
		}
		throw new NotImplementedException();
	}

	protected virtual void Dispose(bool _)
	{
		if ( _disposed )
		{
			return;
		}

		_disposed = true;
	}
}
