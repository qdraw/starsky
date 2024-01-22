using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace starsky.foundation.worker.ThumbnailServices.Exceptions
{
	/// <summary>
	/// @see: EventQueueOverflowException
	/// </summary>
	[Serializable]
	public class ToManyUsageException : Exception
	{
		[SuppressMessage("ReSharper", "RedundantBaseConstructorCall")]
		public ToManyUsageException()
			: base() { }

		public ToManyUsageException(string message)
			: base(message) { }
        
		/// <summary>
		/// Without this constructor, deserialization will fail
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
#pragma warning disable SYSLIB0051
		protected ToManyUsageException(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}
#pragma warning restore SYSLIB0051
	}
}

