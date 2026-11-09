using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Crypto;

namespace starsky.foundation.connect.Discovery;

/// <summary>
/// Implements Syncthing global discovery protocol v3.
/// Announces local addresses to the discovery server and resolves device addresses by ID.
/// </summary>
public sealed class GlobalDiscovery : IHostedService, IDisposable
{
	private static readonly TimeSpan DefaultReannounceInterval = TimeSpan.FromMinutes(30);
	private static readonly TimeSpan ErrorRetryInterval = TimeSpan.FromMinutes(5);

	private readonly string _discoveryServer;
	private readonly string _localDeviceId;
	private readonly Func<string[]> _getAddresses;
	private readonly ILogger<GlobalDiscovery> _logger;
	private readonly HttpClient _httpClient;
	private CancellationTokenSource? _cts;
	private TimeSpan _nextReannounce = DefaultReannounceInterval;

	public GlobalDiscovery(
		string localDeviceId,
		X509Certificate2 clientCert,
		Func<string[]> getAddresses,
		ILogger<GlobalDiscovery> logger,
		string discoveryServer = "https://discovery.syncthing.net/v2/")
	{
		_localDeviceId = DeviceIdentity.NormalizeDeviceId(localDeviceId);
		_getAddresses = getAddresses;
		_logger = logger;
		_discoveryServer = discoveryServer.TrimEnd('/');

		var handler = new HttpClientHandler();
		handler.ClientCertificates.Add(clientCert);
		handler.ServerCertificateCustomValidationCallback =
			HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

		_httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
	}

	/// <summary>Looks up addresses for a device ID. Returns empty array if not found.</summary>
	public async Task<string[]> LookupAsync(string deviceId, CancellationToken ct = default)
	{
		var normalized = DeviceIdentity.NormalizeDeviceId(deviceId);
		try
		{
			var url = $"{_discoveryServer}?device={normalized}";
			var response = await _httpClient.GetFromJsonAsync<LookupResponse>(url, ct);
			return response?.Addresses?.ToArray() ?? [];
		}
		catch ( HttpRequestException ex ) when ( ex.StatusCode == System.Net.HttpStatusCode.NotFound )
		{
			return [];
		}
		catch ( Exception ex )
		{
			_logger.LogWarning(ex, "Global discovery lookup failed for {DeviceId}.", deviceId);
			return [];
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_ = Task.Run(() => AnnounceLoopAsync(_cts.Token), _cts.Token);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_cts?.Cancel();
		return Task.CompletedTask;
	}

	private async Task AnnounceLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			await AnnounceAsync(ct);
			try
			{
				await Task.Delay(_nextReannounce, ct);
			}
			catch ( OperationCanceledException ) { break; }
		}
	}

	private async Task AnnounceAsync(CancellationToken ct)
	{
		try
		{
			var addresses = _getAddresses();
			var body = new AnnounceRequest { Addresses = [.. addresses] };
			var url = $"{_discoveryServer}?id={_localDeviceId}";

			using var response = await _httpClient.PostAsJsonAsync(url, body, ct);

			_nextReannounce = DefaultReannounceInterval;
			if ( response.Headers.TryGetValues("Reannounce-After", out var values) )
			{
				foreach ( var v in values )
				{
					if ( int.TryParse(v, out var seconds) )
					{
						_nextReannounce = TimeSpan.FromSeconds(seconds);
					}
				}
			}
		}
		catch ( Exception ex )
		{
			_logger.LogWarning(ex, "Global discovery announce failed.");
			_nextReannounce = ErrorRetryInterval;
		}
	}

	public void Dispose()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_httpClient.Dispose();
	}

	private sealed class AnnounceRequest
	{
		[JsonPropertyName("addresses")]
		public List<string> Addresses { get; set; } = [];
	}

	private sealed class LookupResponse
	{
		[JsonPropertyName("addresses")]
		public List<string>? Addresses { get; set; }
	}
}
