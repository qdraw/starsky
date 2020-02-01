using System;
using System.IO;

namespace starsky.Health
{
	public class HealthDiskOptionsSetup
	{
		public void Setup(string fullFilePath, DiskStorageOptions diskOptions)
		{
			var directoryInfo = new FileInfo(fullFilePath).Directory;
			if ( directoryInfo == null ) return;
			            
			var tenPercentInBytes = Convert.ToInt64(( ( new DriveInfo(directoryInfo.Root.FullName).TotalFreeSpace /
			                                     1024f ) / 1024f ) * 0.1 );
			if ( tenPercentInBytes < 100 ) return;

			diskOptions.AddDrive(
					directoryInfo.Root.FullName, tenPercentInBytes);
		}

	}
}
