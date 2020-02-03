using System.Collections.Generic;

namespace starsky.Health
{
	public class DiskStorageOptions
	{
		internal Dictionary<string, (string DriveName, long MinimumFreeMegabytes)> ConfiguredDrives { get; } = new Dictionary<string, (string, long)>();

		public void AddDrive(string driveName,
			long minimumFreeMegabytes = 1)
		{
			ConfiguredDrives.Add(driveName, (driveName, minimumFreeMegabytes));
		}
	}
}
