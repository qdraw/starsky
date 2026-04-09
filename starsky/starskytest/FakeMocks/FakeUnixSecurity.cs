using starsky.foundation.mountwatch.ServiceInstaller.Helpers;

namespace starskytest.FakeMocks;

public class FakeUnixSecurity(bool isRoot) : UnixSecurity
{
	public override bool IsRunningAsRoot()
	{
		return isRoot;
	}
}
