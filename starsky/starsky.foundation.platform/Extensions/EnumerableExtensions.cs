using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace starsky.foundation.platform.Extensions
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Running async and return result
		/// @see: https://codereview.stackexchange.com/a/212326
		/// </summary>
		/// <param name="items">!IEnumerableName! (dot) ForEachAsync</param>
		/// <param name="action">Input type</param>
		/// <param name="maxDegreesOfParallelism">Default of 6 task running at the same time </param>
		/// <typeparam name="TSource">output type</typeparam>
		/// <typeparam name="TResult">and output it self</typeparam>
		/// <returns>Task with output</returns>
		public static async Task<IEnumerable<TResult>> ForEachAsync<TSource, TResult>(
			this IEnumerable<TSource> items,
			Func<TSource, Task<TResult>> action,
			int maxDegreesOfParallelism = 6)
		{
			var transformBlock = new TransformBlock<TSource, TResult>(action, new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = maxDegreesOfParallelism
			});

			var bufferBlock = new BufferBlock<TResult>();

			using (transformBlock.LinkTo(bufferBlock, new DataflowLinkOptions {PropagateCompletion = true}))
			{
				foreach (var item in items)
				{
					transformBlock.Post(item);
				}

				transformBlock.Complete();
				await transformBlock.Completion;
			}

			bufferBlock.TryReceiveAll(out var result);
			return result;
		}
	}
}
