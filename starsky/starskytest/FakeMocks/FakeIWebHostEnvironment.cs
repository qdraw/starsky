using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace starskytest.FakeMocks;

/// <summary>
///     Minimal IWebHostEnvironment for tests that build their own service collection,
///     for example the Swagger UI middleware asks for this service while configuring
/// </summary>
public sealed class FakeIWebHostEnvironment : IWebHostEnvironment
{
	public string ApplicationName { get; set; } = "starsky";

	public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

	public string WebRootPath { get; set; } = string.Empty;

	public string EnvironmentName { get; set; } = Environments.Development;

	public string ContentRootPath { get; set; } = string.Empty;

	public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
