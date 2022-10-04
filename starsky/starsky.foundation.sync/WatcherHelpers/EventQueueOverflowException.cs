﻿using System;
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
	        : base(info, context)
        {
        }
    }
}
