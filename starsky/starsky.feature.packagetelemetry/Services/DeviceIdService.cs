using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.Win32;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(IDeviceIdService), InjectionLifetime = InjectionLifetime.Scoped)]
public class DeviceIdService : IDeviceIdService
{
	private readonly ISettingsService _settingsService;
	private readonly IStorage _hostStorage;

	public DeviceIdService(ISelectorStorage selectorStorage, ISettingsService settingsService)
	{
		_settingsService = settingsService;
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}
	
	public string IoReg { get; set; } = "ioreg";
	public string DbusMachineIdPath { get; set; } = "/var/lib/dbus/machine-id";
	public string MachineIdPath2 { get; set; } = "/etc/machine-id";

	public string BsdHostIdPath { get; set; } = "/etc/hostid";

	public async Task<string> DeviceId(OSPlatform? currentPlatform )
	{
		var id = string.Empty;
		if ( currentPlatform == OSPlatform.OSX )
		{
			id = await DeviceIdOsX();
		}
		
		if ( currentPlatform == OSPlatform.Windows )
		{
			id = DeviceIdWindows(currentPlatform);
		}
		
		if ( currentPlatform == OSPlatform.Linux || currentPlatform == OSPlatform.FreeBSD)
		{
			id = await DeviceIdLinuxBsdAsync();
		}

		// For privacy reason this content of this id will be anonymous 
		id = Sha256.ComputeSha256(id);
		
		if ( string.IsNullOrEmpty(id) )
		{
			id = await DeviceIdDatabaseId();
		}
		return id;
	}

	internal async Task<string> DeviceIdDatabaseId()
	{
		var item = await _settingsService.GetSetting(SettingsType.DeviceId);
		if ( !string.IsNullOrEmpty(item?.Value) ) return item.Value;

		var generatedString = $"zz{Sha256.ComputeSha256(FileHash.GenerateRandomBytes(30))}";
		await _settingsService.AddOrUpdateSetting(SettingsType.DeviceId, generatedString);
		return generatedString;
	}

	private async Task<string> DeviceIdLinuxBsdAsync()
	{
		if ( _hostStorage.ExistFile(DbusMachineIdPath) )
		{
			var stream = _hostStorage.ReadStream(DbusMachineIdPath);
			return await StreamToStringHelper.StreamToStringAsync(stream);
		}
		
		if ( _hostStorage.ExistFile(MachineIdPath2) )
		{
			var stream = _hostStorage.ReadStream(MachineIdPath2);
			return await StreamToStringHelper.StreamToStringAsync(stream);
		}
		
		if ( !_hostStorage.ExistFile(BsdHostIdPath) ) return string.Empty;
		var streamBsd = _hostStorage.ReadStream(BsdHostIdPath);
		return await StreamToStringHelper.StreamToStringAsync(streamBsd);
	}

	/// <summary>
	/// Get the device id from the OS
	/// </summary>
	/// <returns>Guid</returns>
	internal async Task<string> DeviceIdOsX()
	{
		// ioreg -rd1 -c IOPlatformExpertDevice
		var result = await Command.Run(IoReg, "-rd1", "-c", "IOPlatformExpertDevice").Task;
		if ( !result.Success ) return string.Empty;
		
		var match = Regex.Match(result.StandardOutput,"\"IOPlatformUUID\" = \"[\\d+\\w+-]+\"",
			RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
		var id = match.Value.Replace("\"IOPlatformUUID\" = \"", "").Replace('\"', ' ').Trim();
		return id;
	}

	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	internal static string DeviceIdWindows(OSPlatform? currentPlatform)
	{
		if (currentPlatform != OSPlatform.Windows)
		{
			return string.Empty;
		}

		try
		{
			// Windows Only feature
			var registryKey =
				Registry.LocalMachine.OpenSubKey(
					@"SOFTWARE\Microsoft\Cryptography");
			var title = registryKey?.GetValue("MachineGuid")?.ToString();
			registryKey?.Dispose();
			return title ?? string.Empty;
		}
		catch ( NullReferenceException )
		{
			return string.Empty;
		}
		catch ( TypeInitializationException )
		{
			return string.Empty;
		}

	}
}


