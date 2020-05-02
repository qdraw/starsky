using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace starsky.foundation.platform.Extensions
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// @see: https://codereview.stackexchange.com/a/212326
		/// </summary>
		/// <param name="items"></param>
		/// <param name="action"></param>
		/// <param name="maxDegreesOfParallelism"></param>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <returns></returns>
		public static async Task<IEnumerable<TResult>> ForEachAsync<TSource, TResult>(
			this IEnumerable<TSource> items,
			Func<TSource, Task<TResult>> action,
			int maxDegreesOfParallelism)
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
