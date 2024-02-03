using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using OpenTelemetry.Proto.Trace.V1;
using ProtoBuf;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Interfaces;

namespace starsky.foundation.webtelemetry.Services;

[Service(typeof(ITracesService), InjectionLifetime = InjectionLifetime.Singleton)]
public class TracesService : ITracesService
{
	private readonly AppSettings _appSettings;
	private readonly IHttpProvider _httpProvider;

	public TracesService(AppSettings appSettings, IHttpProvider httpProvider)
	{
		_appSettings = appSettings;
		_httpProvider = httpProvider;
	}

	private List<KeyValuePair<string, string>> ParseHeader()
	{
		var headers = _appSettings.OpenTelemetry?.GetTracesHeader();
		var metadata = new List<KeyValuePair<string, string>>();
		if ( string.IsNullOrEmpty(headers) ) return metadata;

		var headerArray = headers.Split(',');
		foreach ( var header in headerArray )
		{
			var keyValue = header.Split(header.Contains(':') ? ':' : '=');

			if ( keyValue.Length == 2 )
			{
				metadata.Add(
					new KeyValuePair<string, string>(keyValue[0].Trim(), keyValue[1].Trim()));
			}
		}

		return metadata;
	}

	// todo: add check if TracesEndpoint is set
	
	public async Task SendTrace(TracesData tracesData)
	{
		// Convert telemetryData to Protobuf bytes
		var output = new MemoryStream();
		Serializer.Serialize(output, tracesData);
		
		// Set up the HTTP POST request with Protobuf encoding
		var content = new ByteArrayContent(output.ToArray());
		content.Headers.Add("Content-Type", "application/x-protobuf");
		foreach ( var headerNameValue in ParseHeader() )
		{
			content.Headers.Add(headerNameValue.Key, headerNameValue.Value);
		}

		// Send the HTTP POST request to the OTLP/HTTP endpoint
		var response =
			await _httpProvider.PostAsync(_appSettings.OpenTelemetry?.TracesEndpoint, content);
		
		content.Dispose();

		// Handle the response if needed
		if ( response.IsSuccessStatusCode )
		{
			Console.WriteLine("Telemetry data sent successfully!");
		}
		else
		{
			Console.WriteLine($"Failed to send telemetry data. Status code: {response.StatusCode}");
		}
	}
}
