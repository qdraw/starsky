using Dropbox.Api.Files.Routes;

namespace starsky.feature.cloudimport.Clients.Interfaces;

public interface IDropboxClient : IDisposable
{
	FilesUserRoutes Files { get; }
}
