using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Protocol.Generated;

namespace starsky.foundation.connect.Discovery;

/// <summary>
/// Implements Syncthing local discovery protocol v4.
/// Multicasts device presence on UDP 239.255.68.42:21027 every 30 seconds
/// and caches received announcements for 90 seconds.
/// </summary>
public sealed class LocalDiscovery : IHostedService, IDisposable
{
	private const string MulticastGroup = "239.255.68.42";
	private const int MulticastPort = 21027;
	private static readonly uint LocalMagic = 0x2EA7D90Bu;
	private static readonly TimeSpan BroadcastInterval = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(90);

	private readonly byte[] _localDeviceIdBytes;
	private readonly string _localDeviceId;
	private readonly ILogger<LocalDiscovery> _logger;
	private readonly ulong _instanceId = (ulong)Random.Shared.NextInt64();
	private readonly ConcurrentDictionary<string, (string[] Addresses, DateTimeOffset Expiry)> _cache = new();
	private UdpClient? _udpClient;
	private CancellationTokenSource? _cts;

	public LocalDiscovery(byte[] localDeviceIdBytes, string localDeviceId, ILogger<LocalDiscovery> logger)
	{
		_localDeviceIdBytes = localDeviceIdBytes;
		_localDeviceId = localDeviceId;
		_logger = logger;
	}

	/// <summary>Returns cached LAN addresses for a device, or an empty array if not seen.</summary>
	public string[] Lookup(string deviceId)
	{
		var normalized = DeviceIdentity.NormalizeDeviceId(deviceId);
		if ( _cache.TryGetValue(normalized, out var entry) && entry.Expiry > DateTimeOffset.UtcNow )
		{
			return entry.Addresses;
		}

		return [];
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_udpClient = new UdpClient();
		_udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		_udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));
		_udpClient.JoinMulticastGroup(IPAddress.Parse(MulticastGroup));

		_ = Task.Run(() => SendLoopAsync(_cts.Token), _cts.Token);
		_ = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_cts?.Cancel();
		return Task.CompletedTask;
	}

	private async Task SendLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			try
			{
				await BroadcastAsync(ct);
				await Task.Delay(BroadcastInterval, ct);
			}
			catch ( OperationCanceledException ) { break; }
			catch ( Exception ex ) { _logger.LogWarning(ex, "Local discovery send error."); }
		}
	}

	private async Task BroadcastAsync(CancellationToken ct)
	{
		var addresses = GetLocalAddresses();
		var announce = new Announce
		{
			Id = ByteString.CopyFrom(_localDeviceIdBytes),
			InstanceId = _instanceId,
		};
		announce.Addresses.AddRange(addresses);

		var body = announce.ToByteArray();
		var frame = new byte[4 + body.Length];
		System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(frame, LocalMagic);
		body.CopyTo(frame, 4);

		var endpoint = new IPEndPoint(IPAddress.Parse(MulticastGroup), MulticastPort);
		await _udpClient!.SendAsync(frame, frame.Length, endpoint).WaitAsync(ct);
	}

	private async Task ReceiveLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested )
		{
			try
			{
				var result = await _udpClient!.ReceiveAsync(ct);
				ProcessAnnouncement(result.Buffer, result.RemoteEndPoint);
			}
			catch ( OperationCanceledException ) { break; }
			catch ( Exception ex ) { _logger.LogDebug(ex, "Local discovery receive error."); }
		}
	}

	private void ProcessAnnouncement(byte[] data, IPEndPoint source)
	{
		if ( data.Length < 4 ) return;
		var magic = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data);
		if ( magic != LocalMagic ) return;

		try
		{
			var announce = Announce.Parser.ParseFrom(data, 4, data.Length - 4);
			if ( announce.Id.IsEmpty ) return;

			var deviceId = DeviceIdentity.NormalizeDeviceId(
				Base32.Encode(announce.Id.ToByteArray()));

			// Don't cache ourselves
			if ( deviceId == DeviceIdentity.NormalizeDeviceId(_localDeviceId) ) return;

			// Substitute source IP for unspecified addresses
			var addresses = announce.Addresses
				.Select(a => SubstituteSourceIp(a, source))
				.ToArray();

			_cache[deviceId] = (addresses, DateTimeOffset.UtcNow.Add(CacheLifetime));
		}
		catch ( Exception ex )
		{
			_logger.LogDebug(ex, "Failed to parse local discovery announcement.");
		}
	}

	private static string SubstituteSourceIp(string address, IPEndPoint source)
	{
		// Replace "0.0.0.0" or "::" with the actual source IP
		if ( address.Contains("0.0.0.0", StringComparison.Ordinal) ||
		     address.Contains("[::]", StringComparison.Ordinal) )
		{
			return address.Replace("0.0.0.0", source.Address.ToString(), StringComparison.Ordinal)
			              .Replace("[::]", $"[{source.Address}]", StringComparison.Ordinal);
		}

		return address;
	}

	private static List<string> GetLocalAddresses()
	{
		var addresses = new List<string>();
		foreach ( var iface in NetworkInterface.GetAllNetworkInterfaces() )
		{
			if ( iface.OperationalStatus != OperationalStatus.Up ) continue;
			foreach ( var addr in iface.GetIPProperties().UnicastAddresses )
			{
				if ( addr.Address.AddressFamily != AddressFamily.InterNetwork ) continue;
				if ( IPAddress.IsLoopback(addr.Address) ) continue;
				addresses.Add($"tcp://{addr.Address}:22000");
			}
		}

		return addresses;
	}

	public void Dispose()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_udpClient?.Dispose();
	}
}
