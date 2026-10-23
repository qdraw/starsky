using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.http.Services;

[Service(typeof(ICurlPostHelper), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class CurlPostHelper : ICurlPostHelper
{
	private const int TimeoutInSeconds = 60;
	private const string StatusMarker = "\n%{http_code}";
	private readonly string _curlExecutable;

	public CurlPostHelper() : this("curl")
	{
	}

	/// <param name="curlExecutable">name or path of curl, found with the PATH</param>
	internal CurlPostHelper(string curlExecutable)
	{
		_curlExecutable = curlExecutable;
	}

	public async Task<HttpResponseMessage?> PostAsync(string requestUri, string body,
		string contentType, string? userAgent)
	{
		// the caller checks the allowed domains, this is only a last guard
		if ( !requestUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase) )
		{
			return null;
		}

		var startInfo = new ProcessStartInfo(_curlExecutable)
		{
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		foreach ( var argument in BuildArguments(requestUri, contentType, userAgent) )
		{
			// no shell involved, so the url and headers can not add extra switches
			startInfo.ArgumentList.Add(argument);
		}

		try
		{
			using var process = Process.Start(startInfo);
			if ( process == null )
			{
				return null;
			}

			// the body goes through stdin so it does not show up in the process list
			var outputTask = process.StandardOutput.ReadToEndAsync();
			var errorTask = process.StandardError.ReadToEndAsync();
			await process.StandardInput.WriteAsync(body);
			process.StandardInput.Close();
			await process.WaitForExitAsync();

			await errorTask;
			return ParseOutput(await outputTask);
		}
		catch ( Exception exception ) when ( exception is System.ComponentModel.Win32Exception
			                                    or InvalidOperationException
			                                    or System.IO.IOException )
		{
			// curl is not installed
			return null;
		}
	}

	internal static List<string> BuildArguments(string requestUri, string contentType,
		string? userAgent)
	{
		var arguments = new List<string>
		{
			"--silent",
			"--show-error",
			"--max-time",
			TimeoutInSeconds.ToString(),
			"--request",
			"POST",
			"--header",
			$"Content-Type: {contentType}",
			"--data-binary",
			"@-",
			"--write-out",
			StatusMarker
		};

		if ( !string.IsNullOrWhiteSpace(userAgent) )
		{
			arguments.Add("--user-agent");
			arguments.Add(userAgent);
		}

		arguments.Add("--url");
		arguments.Add(requestUri);
		return arguments;
	}

	/// <summary>
	///     The body is followed by a new line and the status code
	/// </summary>
	internal static HttpResponseMessage? ParseOutput(string output)
	{
		var lastNewLine = output.LastIndexOf('\n');
		if ( lastNewLine < 0 ||
		     !int.TryParse(output[( lastNewLine + 1 )..], out var statusCode) ||
		     statusCode == 0 )
		{
			// 000: no connection
			return null;
		}

		return new HttpResponseMessage(( HttpStatusCode ) statusCode)
		{
			Content = new StringContent(output[..lastNewLine])
		};
	}
}
