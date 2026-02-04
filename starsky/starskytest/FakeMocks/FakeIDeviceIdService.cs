using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.feature.packagetelemetry.Interfaces;

namespace starskytest.FakeMocks;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class FakeIDeviceIdService : IDeviceIdService
{
	public Task<string> DeviceId(OSPlatform? currentPlatform)
	{
		return Task.FromResult("test");
	}
}
