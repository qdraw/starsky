namespace starskytest.starsky.project.web.Helpers.Models;

public class AppSettingsExampleModel
{
	public class KestrelGlobalConfig
	{
		public KestrelConfig? Kestrel { get; set; }
	}

	public class KestrelConfig
	{
		public ExampleEndpoints? Endpoints { get; set; }
	}

	public class ExampleEndpoints
	{
		public EndpointObject? Https { get; set; }
		public EndpointObject? Http { get; set; }
	}

	public class EndpointObject
	{
		public string? Url { get; set; }
	}
}
