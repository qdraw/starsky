namespace starsky.feature.cloudimport.Clients.Interfaces;

public interface IDropboxCloudImportRefreshToken
{
	Task<(string accessToken, int expiresIn)> ExchangeRefreshTokenAsync(
		string refreshToken, string appKey, string appSecret);
}
