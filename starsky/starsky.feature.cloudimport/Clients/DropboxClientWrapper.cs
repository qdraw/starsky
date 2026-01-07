using Dropbox.Api;
using Dropbox.Api.Files.Routes;
using starsky.feature.cloudimport.Clients.Interfaces;

namespace starsky.feature.cloudimport.Clients;

public class DropboxClientWrapper(string accessToken) : IDropboxClient
{
	private readonly DropboxClient _client = new(accessToken);

	public FilesUserRoutes Files => _client.Files;

	public void Dispose()
	{
		_client.Dispose();
		GC.SuppressFinalize(this);
	}
}
