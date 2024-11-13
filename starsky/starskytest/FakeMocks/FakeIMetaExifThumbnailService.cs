using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMetaExifThumbnailService : IMetaExifThumbnailService
{
	public List<(string, string?)> Input { get; set; } =
		Array.Empty<(string, string?)>().ToList();

	public Task<IEnumerable<(bool, bool, string, string?)>> AddMetaThumbnail(
		IEnumerable<(string, string)> subPathsAndHash)
	{
		var subPathsAndHashList = subPathsAndHash.ToList();
		Input.AddRange(subPathsAndHashList!);

		var result = new List<(bool, bool, string, string?)>();
		foreach ( var singleSubPathsAndHash in subPathsAndHashList.ToList() )
		{
			if ( singleSubPathsAndHash.Item1.Contains("fail") )
			{
				result.Add(( false, true, singleSubPathsAndHash.Item1, null ));
				continue;
			}

			result.Add(( true, true, singleSubPathsAndHash.Item1, null ));
		}

		return Task.FromResult(result.AsEnumerable());
	}

	public Task<List<(bool, bool, string, string?)>> AddMetaThumbnail(string subPath)
	{
		Input.Add(( subPath, null ));
		var result = new List<(bool, bool, string, string?)> { ( true, true, subPath, null ) };
		return Task.FromResult(result);
	}

	public Task<(bool, bool, string, string?)> AddMetaThumbnail(string subPath, string fileHash)
	{
		Input.Add(( subPath, fileHash ));
		return Task.FromResult(( true, true, subPath, "" ))!;
	}
}
