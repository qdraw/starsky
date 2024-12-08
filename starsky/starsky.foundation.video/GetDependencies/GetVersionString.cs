using starsky.foundation.http.Interfaces;

namespace starsky.foundation.video.GetDependencies;

public class GetVersionString
{
	private readonly IHttpClientHelper _httpClientHelper;

	public GetVersionString(IHttpClientHelper httpClientHelper)
	{
		_httpClientHelper = httpClientHelper;
	}

	private async Task GetApi()
	{
		await _httpClientHelper.ReadString("");
	}
}
