using Dropbox.Api;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudsync.Clients;

[Service(typeof(ICloudSyncClient), InjectionLifetime = InjectionLifetime.Scoped)]
public class DropboxCloudSyncClient(IWebLogger logger, AppSettings appSettings)
	: ICloudSyncClient
{
	private DropboxClient? _client;
	private string? _currentAccessToken;

	public string Name => "Dropbox";

	public bool Enabled =>
		appSettings.CloudSync.Providers.Any(p =>
			p.Provider.Equals("Dropbox", StringComparison.OrdinalIgnoreCase) &&
			!string.IsNullOrWhiteSpace(p.Credentials.AccessToken));

	public async Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
	{
		EnsureClient();

		var files = new List<CloudFile>();

		try
		{
			var list = await _client!.Files.ListFolderAsync(remoteFolder);

			do
			{
				foreach ( var entry in list.Entries.Where(i => i.IsFile) )
				{
					var fileMetadata = entry.AsFile;
					files.Add(new CloudFile
					{
						Id = fileMetadata.Id,
						Name = fileMetadata.Name,
						Path =
							fileMetadata.PathDisplay ?? fileMetadata.PathLower ?? string.Empty,
						Size = ( long ) fileMetadata.Size,
						ModifiedDate = fileMetadata.ServerModified,
						Hash = fileMetadata.ContentHash ?? string.Empty
					});
				}

				if ( list.HasMore )
				{
					list = await _client.Files.ListFolderContinueAsync(list.Cursor);
				}
				else
				{
					break;
				}
			} while ( true );

			logger.LogInformation(
				$"Listed {files.Count} files from Dropbox folder: {remoteFolder}");
			return files;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Error listing files from Dropbox: {ex.Message}");
			throw;
		}
	}

	public async Task<string> DownloadFileAsync(CloudFile file, string localFolder)
	{
		EnsureClient();

		try
		{
			var localPath = Path.Combine(localFolder, file.Name);

			using var response = await _client!.Files.DownloadAsync(file.Path);
			var content = await response.GetContentAsByteArrayAsync();

			await File.WriteAllBytesAsync(localPath, content);

			logger.LogInformation($"Downloaded file from Dropbox: {file.Name} to {localPath}");
			return localPath;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Error downloading file from Dropbox: {ex.Message}");
			throw;
		}
	}

	public async Task<bool> DeleteFileAsync(CloudFile file)
	{
		EnsureClient();

		try
		{
			await _client!.Files.DeleteV2Async(file.Path);
			logger.LogInformation($"Deleted file from Dropbox: {file.Name}");
			return true;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Error deleting file from Dropbox: {ex.Message}");
			return false;
		}
	}

	public async Task<bool> TestConnectionAsync()
	{
		try
		{
			EnsureClient();
			var account = await _client!.Users.GetCurrentAccountAsync();
			logger.LogInformation($"Successfully connected to Dropbox as {account.Email}");
			return true;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to connect to Dropbox: {ex.Message}");
			return false;
		}
	}

	public void InitializeClient(string accessToken)
	{
		if ( _currentAccessToken == accessToken && _client != null )
		{
			return;
		}

		_client?.Dispose();
		_client = new DropboxClient(accessToken);
		_currentAccessToken = accessToken;
	}

	private void EnsureClient()
	{
		if ( _client != null )
		{
			return;
		}

		throw new InvalidOperationException(
			"Dropbox client not initialized. Call InitializeClient with access token first.");
	}
}
