using starsky.foundation.platform.Models.Kestrel;

namespace starsky.foundation.platform.Models
{
	public sealed class AppContainerAppSettings
	{
		public KestrelContainer? Kestrel { get; set; }
		public AppSettings App { get; set; } = new AppSettings();
	}
}
