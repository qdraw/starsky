using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.Win32;
using starsky.feature.packagetelemetry.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.packagetelemetry.Services;

[Service(typeof(IDeviceIdService), InjectionLifetime = InjectionLifetime.Scoped)]
public class DeviceIdService : IDeviceIdService
{
	private readonly IStorage _hostStorage;

	public DeviceIdService(ISelectorStorage selectorStorage)
	{
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}
	
	public string IoReg { get; set; } = "ioreg";
	public string DbusMachineIdPath { get; set; } = "/var/lib/dbus/machine-id";

	public string BsdHostIdPath { get; set; } = "/etc/hostid";

	public string FallBackId { get; set; } = "not set";

	public async Task<string> DeviceId(OSPlatform? currentPlatform )
	{
		var id = FallBackId;
		if ( currentPlatform == OSPlatform.OSX )
		{
			id = await DeviceIdOsX();
		}
		
		if ( currentPlatform == OSPlatform.Windows )
		{
			id = DeviceIdWindows();
		}
		
		if ( currentPlatform == OSPlatform.Linux || currentPlatform == OSPlatform.FreeBSD)
		{
			id = DeviceIdLinuxBsd();
		}
		
		if ( string.IsNullOrEmpty(id) )
		{
			id = FallBackId;
		}
		return id;
	}

	private string DeviceIdLinuxBsd()
	{
		if ( _hostStorage.ExistFile(DbusMachineIdPath) )
		{
			var stream = _hostStorage.ReadStream(DbusMachineIdPath);
			return PlainTextFileHelper.StreamToString(stream);
		}

		if ( !_hostStorage.ExistFile(BsdHostIdPath) ) return FallBackId;
		var streamBsd = _hostStorage.ReadStream(BsdHostIdPath);
		return PlainTextFileHelper.StreamToString(streamBsd);
	}

	/// <summary>
	/// Get the device id from the OS
	/// </summary>
	/// <returns>Guid</returns>
	private async Task<string> DeviceIdOsX()
	{
		var id = "not set";
		var result = await Command.Run(IoReg, "-rd1", "-c", "IOPlatformExpertDevice").Task;
		if ( !result.Success ) return id;
		
		var match = Regex.Match(result.StandardOutput,"\"IOPlatformUUID\" = \"[\\d+\\w+-]+\"",
			RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
		id = match.Value.Replace("\"IOPlatformUUID\" = \"", "").Replace('\"', ' ').Trim();
		return id;
	}

	private static string? DeviceIdWindows()
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return string.Empty;
		}
		
#pragma warning disable CS8600
		var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
#pragma warning restore CS8600s

		var title = registryKey?.GetValue("MachineGuid")?.ToString();
		registryKey?.Dispose();

		return title;
	}
}


