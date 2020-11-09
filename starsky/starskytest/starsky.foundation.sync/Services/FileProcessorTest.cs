using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.sync.Services
{
	[TestClass]
	public class FileProcessorTest
	{
		private List<string> IsExecuted { get; set; } = new List<string>();

#pragma warning disable 1998
		private async Task<List<FileIndexItem>> TestExecuted(string filePath, bool test)
#pragma warning restore 1998
		{
			IsExecuted.Add(filePath);
			return new List<FileIndexItem>();
		}
		
		[TestMethod]
		[Timeout(1000)]
		public async Task FileProcessor_CheckInput()
		{
			var fileProcessor = new FileProcessor(TestExecuted);

			const string path = "/test";
			fileProcessor.QueueInput(path);

			var workerThread =
				new Thread(fileProcessor.EndlessWorkQueue) {Priority = ThreadPriority.BelowNormal};
			workerThread.Start();

			await Task.Delay(10);
			
			Assert.IsTrue(IsExecuted.Contains(path));
			
			workerThread.Interrupt();
				
			IsExecuted = new List<string>();
		}
		
	}
}
