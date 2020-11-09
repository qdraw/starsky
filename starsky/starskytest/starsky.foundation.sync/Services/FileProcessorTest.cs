using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
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
		[Timeout(400)]
		public void CheckInput()
		{
			var fileProcessor = new FileProcessor(TestExecuted);
			
			fileProcessor.QueueInput(new CreateAnImage().BasePath);
			fileProcessor.QueueInput("/test2");
			
			fileProcessor.EndlessWorkQueue(false);

			Assert.AreEqual(2, IsExecuted.Count);
			Assert.IsTrue(IsExecuted.Contains(new CreateAnImage().BasePath));
			Assert.IsTrue(IsExecuted.Contains("/test2"));

			IsExecuted = new List<string>();
		}

		[TestMethod]
		[Timeout(100)]
		[UnitTestOutcome()]
		public void CheckInput2()
		{
			var fileProcessor = new FileProcessor(TestExecuted);
			
			fileProcessor.QueueInput(new CreateAnImage().BasePath);
			fileProcessor.QueueInput("/test2");
			
			fileProcessor.EndlessWorkQueue(true);

			Assert.AreEqual(2, IsExecuted.Count);
			Assert.IsTrue(IsExecuted.Contains(new CreateAnImage().BasePath));
			Assert.IsTrue(IsExecuted.Contains("/test2"));

			IsExecuted = new List<string>();
		}
	}
}
