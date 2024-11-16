using starsky.foundation.http.Interfaces;

namespace starsky.foundation.video.GetDependencies;

public class GetVersionString
{
	private readonly IHttpClientHelper _httpClientHelper;

	public GetVersionString(IHttpClientHelper httpClientHelper)
	{
		_httpClientHelper = httpClientHelper;
	}
	
	/// <summary>
	/// https://www.osxexperts.net/
	/// </summary>
	private const string OsxArm64 = "https://www.osxexperts.net/ffmpeg71arm.zip";

	private const string FfBinariesApi = "https://ffbinaries.com/api/v1/version/6.1";
	
	public string GetVersion(
		OperationSystemPlatforms.OSPlatformAndArchitecture osPlatformAndArchitecture)
	{
		switch ( osPlatformAndArchitecture )
		{
			case OperationSystemPlatforms.OSPlatformAndArchitecture.WinX86:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.WinX64:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.WinArm64:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.LinuxX64:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.LinuxArm:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.LinuxArm64:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.OsxX64:
				break;
			case OperationSystemPlatforms.OSPlatformAndArchitecture.OsxArm64:
				return OsxArm64;
			default:
				throw new ArgumentOutOfRangeException(nameof(osPlatformAndArchitecture),
					osPlatformAndArchitecture, null);
		}
	}

	private void GetApi()
	{
		await _httpClientHelper.ReadString(FfBinariesApi);
		
	}
}
