using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace starsky.feature.packagetelemetry.Interfaces;

public interface IDeviceIdService
{
	Task<string> DeviceId(OSPlatform? currentPlatform);
}
