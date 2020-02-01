using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace starsky.Health
{
public class DiskStorageHealthCheck : IHealthCheck
  {
    private readonly DiskStorageOptions _options;

    public DiskStorageHealthCheck(DiskStorageOptions options)
    {
      DiskStorageOptions diskStorageOptions = options;
      if (diskStorageOptions == null)
        throw new ArgumentNullException(nameof (options));
      this._options = diskStorageOptions;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default (CancellationToken))
    {
      try
      {
        foreach ((string, long) valueTuple in this._options.ConfiguredDrives.Values)
        {
          string driveName = valueTuple.Item1;
          long num = valueTuple.Item2;
          (bool Exists4, long ActualFreeMegabytes4) = DiskStorageHealthCheck.GetSystemDriveInfo(driveName);
          if (!Exists4)
            return Task.FromResult<HealthCheckResult>(new HealthCheckResult(context.Registration.FailureStatus, "Configured drive " + driveName + " is not present on system", (Exception) null, (IReadOnlyDictionary<string, object>) null));
          if (ActualFreeMegabytes4 < num)
            return Task.FromResult<HealthCheckResult>(new HealthCheckResult(context.Registration.FailureStatus, string.Format("Minimum configured megabytes for disk {0} is {1} but actual free space are {2} megabytes", (object) driveName, (object) num, (object) ActualFreeMegabytes4), (Exception) null, (IReadOnlyDictionary<string, object>) null));
        }
        return Task.FromResult<HealthCheckResult>(HealthCheckResult.Healthy((string) null, (IReadOnlyDictionary<string, object>) null));
      }
      catch (Exception ex)
      {
        return Task.FromResult<HealthCheckResult>(new HealthCheckResult(context.Registration.FailureStatus, (string) null, ex, (IReadOnlyDictionary<string, object>) null));
      }
    }

    private static (bool Exists, long ActualFreeMegabytes) GetSystemDriveInfo(string driveName)
    {
      DriveInfo driveInfo = ((IEnumerable<DriveInfo>) DriveInfo.GetDrives()).FirstOrDefault<DriveInfo>((Func<DriveInfo, bool>) (drive => string.Equals(drive.Name, driveName, StringComparison.InvariantCultureIgnoreCase)));
      return driveInfo != null ? (true, driveInfo.AvailableFreeSpace / 1024L / 1024L) : (false, 0L);
    }
  }
}
