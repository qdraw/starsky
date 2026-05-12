using System.Collections.Generic;

namespace starsky.feature.health.HealthCheck;

public class DiskStorageOptions
{
	internal Dictionary<string, (string DriveName, long MinimumFreeMegabytes)> ConfiguredDrives
	{
		get;
	} = new();

	public void AddDrive(string driveName,
		long minimumFreeMegabytes = 1)
	{
		ConfiguredDrives.Add(driveName, ( driveName, minimumFreeMegabytes ));
	}
}
