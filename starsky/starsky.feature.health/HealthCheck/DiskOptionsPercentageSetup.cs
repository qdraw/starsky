using System;
using System.IO;

namespace starsky.feature.health.HealthCheck
{
	public static class DiskOptionsPercentageSetup
	{
		/// <summary>
		/// On the disk must be at least 10% free. 10% is the default value
		/// </summary>
		/// <param name="fullFilePath">full file path</param>
		/// <param name="diskOptions">to add to this object</param>
		/// <param name="percentage">(optional) between 0 and 1</param>
		public static void Setup(string fullFilePath, DiskStorageOptions diskOptions, float percentage = 0.1f)
		{
			var directoryInfo = new FileInfo(fullFilePath).Directory;
			if ( directoryInfo == null )
			{
				return;
			}

			var tenPercentInBytes = Convert.ToInt64(( ( new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace /
												 1024f ) / 1024f ) * percentage);
			if ( tenPercentInBytes < 100 )
			{
				return;
			}

			diskOptions.AddDrive(
					directoryInfo.Root.FullName, tenPercentInBytes);
		}

	}
}
