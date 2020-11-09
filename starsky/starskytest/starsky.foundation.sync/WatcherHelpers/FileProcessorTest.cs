using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherHelpers;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
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
		public async Task FileProcessor_CheckInput_newThread()
		{
			var fileProcessor = new FileProcessor(TestExecuted);

			const string path = "/test";
			fileProcessor.QueueInput(path);

			var workerThread =
				new Thread(fileProcessor.EndlessWorkQueue) {Priority = ThreadPriority.BelowNormal};
			workerThread.Start();

			await Task.Delay(10);
			
			Assert.AreEqual(1,IsExecuted.Count);
			Assert.IsTrue(IsExecuted.Contains(path));
			
			workerThread.Interrupt();
				
			IsExecuted = new List<string>();
		}
		
		[TestMethod]
		[Timeout(1000)]
		public async Task FileProcessor_CheckInput_existingThread()
		{
			var fileProcessor = new FileProcessor(TestExecuted);

			const string path = "/test";
			fileProcessor.QueueInput(path);

			var workerThread =
				new Thread(fileProcessor.EndlessWorkQueue) {Priority = ThreadPriority.BelowNormal};
			workerThread.Start();

			await Task.Delay(5);
			
			fileProcessor.QueueInput(path + "/2");
			
			await Task.Delay(10);

			Assert.AreEqual(2,IsExecuted.Count);
			Assert.IsTrue(IsExecuted.Contains(path));
			Assert.IsTrue(IsExecuted.Contains(path+ "/2"));

			workerThread.Interrupt();
			
			IsExecuted = new List<string>();
		}
		
	}
}
