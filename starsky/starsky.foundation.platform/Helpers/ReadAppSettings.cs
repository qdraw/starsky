using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers;

public static class ReadAppSettings
{
	internal static async Task<AppContainerAppSettings> Read(string path)
	{
		if ( !File.Exists(path) )
		{
			return new AppContainerAppSettings();
		}
			
		using ( var openStream = File.OpenRead(path) )
		{
			return  await JsonSerializer
				.DeserializeAsync<AppContainerAppSettings>(openStream,
					new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true,
						Converters =
						{
							new JsonBoolQuotedConverter(),
						},
					});
		}
	}
}