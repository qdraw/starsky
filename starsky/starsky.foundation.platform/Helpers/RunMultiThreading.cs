using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.foundation.platform.Helpers
{
	public static class RunMultiThreading
	{
		public static async Task RunWithMaxDegreeOfConcurrency<T>(int maxDegreeOfConcurrency, 
			IEnumerable<T> collection, Func<T, Task> taskFactory)
		{
			var activeTasks = new List<Task>(maxDegreeOfConcurrency);
			foreach (var task in collection.Select(taskFactory))
			{
				activeTasks.Add(task);
				if (activeTasks.Count == maxDegreeOfConcurrency)
				{
					await Task.WhenAny(activeTasks.ToArray());
					//observe exceptions here
					activeTasks.RemoveAll(t => t.IsCompleted); 
				}
			}
			await Task.WhenAll(activeTasks.ToArray()).ContinueWith(t => 
			{
				//observe exceptions in a manner consistent with the above   
			});
		}
	}
}
