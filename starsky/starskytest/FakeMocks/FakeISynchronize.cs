using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeISynchronize : ISynchronize
	{
		public event EventHandler<string> Receive;

		public List<Tuple<string, bool>> Inputs { get; set; } = new List<Tuple<string, bool>>();
		
		public Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			Console.WriteLine($"sync => {subPath}");
			Inputs.Add(new Tuple<string, bool>(subPath,recursive));
			Receive?.Invoke(this, subPath);
			return Task.FromResult(new List<FileIndexItem>());
		}

		public Task<List<FileIndexItem>> SingleFile(string subPath)
		{
			throw new NotImplementedException();
			return Task.FromResult(new List<FileIndexItem>());
		}
	}
}
