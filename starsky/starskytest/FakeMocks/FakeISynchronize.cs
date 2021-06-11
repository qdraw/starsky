using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeISynchronize : ISynchronize
	{
		private readonly List<FileIndexItem> _data = new List<FileIndexItem>();

		public FakeISynchronize(List<FileIndexItem> data = null)
		{
			if ( data != null ) _data = data;
		}
		
		public event EventHandler<string> Receive;

		public List<Tuple<string, bool>> Inputs { get; set; } = new List<Tuple<string, bool>>();
		
		public Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true,
			ISynchronize.SocketUpdateDelegate updateDelegate = null)
		{
			Console.WriteLine($"sync => {subPath}");
			Inputs.Add(new Tuple<string, bool>(subPath,recursive));
			Receive?.Invoke(this, subPath);
			return Task.FromResult(_data);
		}
		public async Task<List<FileIndexItem>> Sync(List<string> subPaths, bool recursive = true)
		{
			var results = new List<FileIndexItem>();
			foreach ( var subPath in subPaths )
			{
				results.AddRange(await Sync(subPath, recursive));
			}
			return results;
		}
	}
}
