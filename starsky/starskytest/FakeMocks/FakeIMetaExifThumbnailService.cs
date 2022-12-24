using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.metathumbnail.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIMetaExifThumbnailService : IMetaExifThumbnailService
	{
		public List<(string, string)> Input { get; set; } =
			Array.Empty<(string, string)>().ToList();
		
		public Task<IEnumerable<(bool,string)>>  AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash)
		{
			var subPathsAndHashList = subPathsAndHash.ToList();
			Input.AddRange(subPathsAndHashList);

			var result = new List<(bool, string)>();
			foreach ( var singleSubPathsAndHash in subPathsAndHashList.ToList() )
			{
				if ( singleSubPathsAndHash.Item1.Contains("fail") )
				{
					result.Add((false, singleSubPathsAndHash.Item1));
					continue;
				}
				
				result.Add((true, singleSubPathsAndHash.Item1));
			}
			
			return Task.FromResult(result.AsEnumerable());
		}

		public Task<List<(bool,string)>> AddMetaThumbnail(string subPath)
		{
			Input.Add((subPath, null));
			var result = new List<(bool, string)> { (true, subPath) };
			return Task.FromResult(result);
		}

		public Task<(bool,string)> AddMetaThumbnail(string subPath, string fileHash)
		{
			Input.Add((subPath, fileHash));
			return Task.FromResult((true, subPath));
		}
	}
}
