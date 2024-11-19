using starsky.foundation.http.Interfaces;

namespace starsky.foundation.video.GetDependencies;

public class GetVersionString
{
	/// <summary>
	///     https://www.osxexperts.net/
	/// </summary>
	private const string OsxArm64 = "https://www.osxexperts.net/ffmpeg71arm.zip";

	private const string FfBinariesApi = "https://ffbinaries.com/api/v1/version/6.1";
	private readonly IHttpClientHelper _httpClientHelper;

	public GetVersionString(IHttpClientHelper httpClientHelper)
	{
		_httpClientHelper = httpClientHelper;
	}

	private async Task GetApi()
	{
		await _httpClientHelper.ReadString(FfBinariesApi);
	}
}
