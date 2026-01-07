using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using starsky.feature.cloudimport.Clients.Interfaces;

namespace starsky.feature.cloudimport.Clients;

public class DropboxClientWrapper(string accessToken) : IDropboxClient
{
	private readonly DropboxClient _client = new(accessToken);

	public async Task<ListFolderResult> ListFolderAsync(string path)
	{
		return await _client.Files.ListFolderAsync(path);
	}

	public async Task<ListFolderResult> ListFolderContinueAsync(string arg)
	{
		return await _client.Files.ListFolderContinueAsync(arg);
	}

	public async Task<IDownloadResponse<FileMetadata>> DownloadAsync(string path,
		string? rev = null)
	{
		return await _client.Files.DownloadAsync(path, rev);
	}

	public async Task<DeleteResult> DeleteV2Async(string path, string? parentRev = null)
	{
		return await _client.Files.DeleteV2Async(path, parentRev);
	}

	public void Dispose()
	{
		_client.Dispose();
		GC.SuppressFinalize(this);
	}
}
