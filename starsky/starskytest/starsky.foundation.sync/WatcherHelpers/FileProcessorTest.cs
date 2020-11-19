using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.sync.WatcherHelpers;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class FileProcessorTest
	{
		private List<string> IsExecuted { get; set; } = new List<string>();

		private Task<List<FileIndexItem>> TestExecuted(Tuple<string, WatcherChangeTypes> value)
		{
			IsExecuted.Add(value.Item1);
			return Task.FromResult(new List<FileIndexItem>());
		}

		[TestMethod]
		[Timeout(9000)]
		public async Task FileProcessor_EndlessWorkQueueAsync()
		{
			CancellationTokenSource source = new CancellationTokenSource();
			source.Cancel();
			var fileProcessor = new FileProcessor(TestExecuted);
			await fileProcessor.EndlessWorkQueueAsync(source.Token);
		}

		[TestMethod]
		[Timeout(9000)]
		public async Task FileProcessor_QueueInput()
		{
			var fileProcessor = new FileProcessor(TestExecuted);

			const string path = "/test";
			fileProcessor.QueueInput(path, WatcherChangeTypes.Changed);

			await Task.Delay(5);
			
			fileProcessor.QueueInput(path+ "/2", WatcherChangeTypes.Changed);

			await Task.Delay(10);

			Assert.AreEqual(2,IsExecuted.Count);
			Assert.IsTrue(IsExecuted.Contains(path));
			Assert.IsTrue(IsExecuted.Contains(path+ "/2"));
			
			IsExecuted = new List<string>();
		}
		
	}
}
