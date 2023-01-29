#nullable enable
namespace starsky.foundation.platform.Models.Kestrel;

public sealed class KestrelContainerEndpoints
{
	public KestrelContainerEndpointsUrl? Https { get; set; }
	public KestrelContainerEndpointsUrl? Http { get; set; }

}
