using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace starsky.foundation.sync.WatcherHelpers
{
	[Serializable]
    public class EventQueueOverflowException : Exception
    {
        [SuppressMessage("ReSharper", "RedundantBaseConstructorCall")]
        public EventQueueOverflowException()
            : base() { }

        public EventQueueOverflowException(string message)
            : base(message) { }
        
        /// <summary>
        /// Without this constructor, deserialization will fail
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventQueueOverflowException(SerializationInfo info, StreamingContext context) 
#pragma warning disable SYSLIB0051
	        : base(info, context)
#pragma warning restore SYSLIB0051
        {
        }
    }
}
