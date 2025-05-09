using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Architecture;
using starsky.foundation.writemeta.Interfaces;

namespace starskytest.FakeMocks;

public class FakeExifToolDownload : IExifToolDownload
{
	public List<bool> Called { get; set; } = new();

	public async Task<List<bool>> DownloadExifTool(List<string> architectures)
	{
		if ( architectures.Count == 0 )
		{
			architectures.Add(CurrentArchitecture.GetCurrentRuntimeIdentifier());
		}

		var result = new List<bool>();
		foreach ( var architecture in architectures.Where(p => p !=
		                                                       DotnetRuntimeNames
			                                                       .GenericRuntimeName) )
		{
			var isWindows = DotnetRuntimeNames.IsWindows(architecture);
			await DownloadExifTool(isWindows);
		}

		return result;
	}

	public Task<bool> DownloadExifTool(bool isWindows, int minimumSize = 30)
	{
		Called.Add(true);
		return Task.FromResult(true);
	}
}
