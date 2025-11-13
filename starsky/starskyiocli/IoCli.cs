using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.JsonConverter;

namespace starskyiocli;

public class IoCli(ServiceProvider serviceProvider)
{
	public async Task Invoke()
	{
		var mapper = new ControllerMapper();

		while ( true )
		{
			var line = Console.ReadLine();
			if ( line == null )
			{
				break;
			}

			try
			{
				var req = JsonSerializer.Deserialize<RequestIoModel>(line,
					DefaultJsonSerializer.CamelCaseNoEnters);

				if ( req == null )
				{
					break;
				}

				var result = mapper.Invoke(req.Method, req.Path, req.Parameters, serviceProvider);
				Console.WriteLine(JsonSerializer.Serialize(new ResponseIoModel
				{
					IsSuccess = true, Data = result
				}));
			}
			catch ( Exception ex )
			{
				Console.WriteLine(
					JsonSerializer.Serialize(new ResponseIoModel
					{
						IsSuccess = false, Data = ex.Message
					}));
			}
		}
	}
}
