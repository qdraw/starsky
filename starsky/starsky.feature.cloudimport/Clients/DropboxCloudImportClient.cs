using Dropbox.Api;
using starsky.foundation.cloudimport.Clients.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudimport.Clients;

[Service(typeof(ICloudImportClient), InjectionLifetime = InjectionLifetime.Scoped)]
public class DropboxCloudImportClient(
	IWebLogger logger,
	AppSettings appSettings,
	IDropboxCloudImportRefreshToken tokenClient)
	: ICloudImportClient
{
	private DateTimeOffset? _accessTokenExpiry;
	private DropboxClient? _client;

	public string Name => "Dropbox";

	public bool Enabled =>
		appSettings.CloudImport?.Providers.Any(p =>
			p.Provider.Equals("Dropbox", StringComparison.OrdinalIgnoreCase) &&
			!string.IsNullOrWhiteSpace(p.Credentials.RefreshToken)) == true;

	public async Task<List<CloudFile>> ListFilesAsync(string remoteFolder)
	{
		EnsureClient();

		var files = new List<CloudFile>();

		try
		{
			var list = await _client!.Files.ListFolderAsync(remoteFolder);

			do
			{
				files.AddRange(list.Entries
					.Where(i => i.IsFile)
					.Select(entry =>
					{
						var fileMetadata = entry.AsFile;
						return new CloudFile
						{
							Id = fileMetadata.Id,
							Name = fileMetadata.Name,
							Path = fileMetadata.PathDisplay ?? fileMetadata.PathLower ?? string.Empty,
							Size = (long)fileMetadata.Size,
							ModifiedDate = fileMetadata.ServerModified,
							Hash = fileMetadata.ContentHash ?? string.Empty
						};
					}));

				if (list.HasMore)
				{
					list = await _client.Files.ListFolderContinueAsync(list.Cursor);
				}
				else
				{
					break;
				}
			} while (true);

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
			var result = await _client!.Files.ListFolderAsync(string.Empty);
			logger.LogInformation(
				$"Successfully connected to Dropbox that has {result.Entries.Count} files");
			return true;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Failed to connect to Dropbox: {ex.Message}");
			return false;
		}
	}


	/// <summary>
	///     Initializes the Dropbox client using a refresh token (preferred)
	/// </summary>
	public async Task InitializeClient(string refreshToken, string appKey, string appSecret)
	{
		// Only refresh if no token or expired
		if ( _client != null && _accessTokenExpiry.HasValue &&
		     _accessTokenExpiry > DateTimeOffset.UtcNow.AddMinutes(1) )
		{
			return;
		}

		var (accessToken, expiresIn) = await tokenClient.ExchangeRefreshTokenAsync(refreshToken,
			appKey,
			appSecret);
		_client?.Dispose();
		_client = new DropboxClient(accessToken);
		_accessTokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 60); // buffer
	}

	private void EnsureClient()
	{
		if ( _client != null )
		{
			return;
		}

		throw new InvalidOperationException(
			"Dropbox client not initialized. Call InitializeClient first.");
	}
}
