using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.connect.Sync;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Storage;
using starsky.foundation.database.Data;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.Controllers;

[Authorize]
public sealed class ConnectController : Controller
{
	private readonly ApplicationDbContext _db;

	public ConnectController(ApplicationDbContext db)
	{
		_db = db;
	}

	internal sealed class ConnectConfigResponse
	{
		public string DeviceId { get; set; } = string.Empty;
		public string DeviceName { get; set; } = string.Empty;
		public List<ConnectDeviceResponse> Devices { get; set; } = [];
		public List<ConnectFolderResponse> Folders { get; set; } = [];
	}

	internal sealed class ConnectDeviceResponse
	{
		public string Id { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public List<string> Addresses { get; set; } = [];
	}

	internal sealed class ConnectFolderResponse
	{
		public string Id { get; set; } = string.Empty;
		public string Label { get; set; } = string.Empty;
		public string Path { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
	}

	public sealed class AddDeviceRequest
	{
		public string DeviceId { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public List<string> Addresses { get; set; } = [];
	}

	/// <summary>Returns the local device ID, device name, known peers and folders.</summary>
	/// <response code="200">Config</response>
	/// <response code="401">Not authenticated</response>
	[HttpGet("/api/connect/config")]
	[Permission(UserManager.AppPermissions.AppSettingsWrite)]
	[Produces("application/json")]
	[ProducesResponseType(typeof(ConnectConfigResponse), 200)]
	[ProducesResponseType(401)]
	public async Task<IActionResult> GetConfig()
	{
		var store = ConfigStore.Default(_db);
		var config = await store.LoadAsync();

		if ( string.IsNullOrEmpty(config.CertificatePfxBase64) )
		{
			var newCert = DeviceIdentity.GenerateCertificate();
			config.CertificatePfxBase64 = Convert.ToBase64String(newCert.Export(X509ContentType.Pfx));
			await store.SaveAsync(config);
		}

		var certBytes = Convert.FromBase64String(config.CertificatePfxBase64);
		var x509 = X509CertificateLoader.LoadPkcs12(certBytes, password: null);
		var deviceId = DeviceIdentity.DeriveDeviceId(x509);

		return Json(new ConnectConfigResponse
		{
			DeviceId = deviceId,
			DeviceName = config.DeviceName,
			Devices = config.Devices
				.Select(d => new ConnectDeviceResponse
				{
					Id = d.Id,
					Name = d.Name,
					Addresses = d.Addresses,
				})
				.ToList(),
			Folders = config.Folders
				.Select(f => new ConnectFolderResponse
				{
					Id = f.Id,
					Label = f.Label,
					Path = f.Path,
					Type = f.Type,
				})
				.ToList(),
		});
	}

	/// <summary>Adds a remote device to the allow-list.</summary>
	/// <response code="200">Device added</response>
	/// <response code="400">Missing or invalid DeviceId</response>
	/// <response code="409">Device already in list</response>
	/// <response code="401">Not authenticated</response>
	[HttpPost("/api/connect/device")]
	[Permission(UserManager.AppPermissions.AppSettingsWrite)]
	[Produces("application/json")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(typeof(string), 409)]
	[ProducesResponseType(401)]
	public async Task<IActionResult> AddDevice([FromBody] AddDeviceRequest request)
	{
		if ( string.IsNullOrWhiteSpace(request.DeviceId) )
		{
			return BadRequest("DeviceId is required");
		}

		var normalizedId = DeviceIdentity.NormalizeDeviceId(request.DeviceId);
		if ( normalizedId.Length != 56 )
		{
			return BadRequest("Invalid DeviceId: must be a 63-character Syncthing Device ID");
		}

		var store = ConfigStore.Default(_db);
		var config = await store.LoadAsync();

		if ( config.Devices.Any(d => DeviceIdentity.NormalizeDeviceId(d.Id) == normalizedId) )
		{
			return Conflict("Device already in list");
		}

		config.Devices.Add(new ConnectDeviceConfig
		{
			Id = normalizedId,
			Name = request.Name,
			Addresses = request.Addresses,
		});

		await store.SaveAsync(config);
		return Ok();
	}

	/// <summary>Removes a remote device from the allow-list.</summary>
	/// <response code="200">Device removed</response>
	/// <response code="404">Device not found</response>
	/// <response code="401">Not authenticated</response>
	[HttpDelete("/api/connect/device/{deviceId}")]
	[Permission(UserManager.AppPermissions.AppSettingsWrite)]
	[Produces("application/json")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(401)]
	public async Task<IActionResult> RemoveDevice(string deviceId)
	{
		var normalizedId = DeviceIdentity.NormalizeDeviceId(deviceId);
		var store = ConfigStore.Default(_db);
		var config = await store.LoadAsync();

		var removed = config.Devices.RemoveAll(d =>
			DeviceIdentity.NormalizeDeviceId(d.Id) == normalizedId);

		if ( removed == 0 )
		{
			return NotFound();
		}

		await store.SaveAsync(config);
		return Ok();
	}
}
