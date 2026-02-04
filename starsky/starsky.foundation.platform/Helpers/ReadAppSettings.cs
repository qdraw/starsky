using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers;

public static class ReadAppSettings
{
	public static async Task<AppContainerAppSettings?> Read(string path)
	{
		if ( !File.Exists(path) )
		{
			return new AppContainerAppSettings();
		}

		using ( var openStream = File.OpenRead(path) )
		{
			var result = await JsonSerializer.DeserializeAsync<AppContainerAppSettings>(
				openStream, DefaultJsonSerializer.NoNamingPolicyBoolAsString);

			return result;
		}
	}
}
