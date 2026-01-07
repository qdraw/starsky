using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace starsky.feature.cloudimport.Clients.Interfaces;

public interface IDropboxClient : IDisposable
{
	Task<ListFolderResult> ListFolderAsync(string path);

	Task<ListFolderResult> ListFolderContinueAsync(string arg);

	Task<IDownloadResponse<FileMetadata>> DownloadAsync(string path, string? rev = null);

	Task<DeleteResult> DeleteV2Async(string path, string? parentRev = null);
}
