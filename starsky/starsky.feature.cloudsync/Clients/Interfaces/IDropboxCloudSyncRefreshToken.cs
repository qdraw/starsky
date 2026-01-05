namespace starsky.foundation.cloudsync.Clients.Interfaces;

public interface IDropboxCloudSyncRefreshToken
{
	Task<(string accessToken, int expiresIn)> ExchangeRefreshTokenAsync(
		string refreshToken, string appKey, string appSecret);
}
