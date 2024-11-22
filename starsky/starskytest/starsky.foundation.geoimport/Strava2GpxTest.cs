using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.geoimport;

namespace starskytest.starsky.foundation.geoimport;

[TestClass]
public class Strava2GpxTest
{
	[TestMethod]
	public async Task Strava2Gpx_ConstructorTest()
	{
		var strava2Gpx = new Strava2Gpx("107059", "","");

		await strava2Gpx.ConnectAsync();
		await strava2Gpx.WriteToGpxAsync(12866651873);
	}
}
