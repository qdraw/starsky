using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class PortProgramHelperTest
{
	private readonly string _prePort;
	private readonly string _preAspNetUrls;

	public PortProgramHelperTest()
	{
		_prePort = Environment.GetEnvironmentVariable("PORT");
		_preAspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
	}
	
	[TestMethod]
	public async Task SetEnvPortAspNetUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");

		await PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());
		Assert.AreEqual("http://*:8000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public async Task SetEnvPortAspNetUrlsAndSetDefault_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","8000");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
	
		await PortProgramHelper.SetEnvPortAspNetUrlsAndSetDefault(Array.Empty<string>());
		Assert.AreEqual("http://*:8000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public async Task SetEnvPortAspNetUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");

		await PortProgramHelper.SetEnvPortAspNetUrls(new List<string>());
		Assert.AreEqual(null,Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
		
	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldSet()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","");
	
		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());
		Assert.AreEqual("http://localhost:4000;https://localhost:4001",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
	
	[TestMethod]
	public void SetDefaultAspNetCoreUrls_ShouldIgnore()
	{
		Environment.SetEnvironmentVariable("PORT","");
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","http://localhost:4000");
	
		PortProgramHelper.SetDefaultAspNetCoreUrls(Array.Empty<string>());
		Assert.AreEqual("http://localhost:4000",Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
	}
}
