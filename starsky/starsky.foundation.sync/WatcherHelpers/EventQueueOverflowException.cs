using System;

namespace starsky.foundation.sync.WatcherHelpers
{
	[Serializable]
    public class EventQueueOverflowException : Exception
    {
        public EventQueueOverflowException()
            : base() { }

        public EventQueueOverflowException(string message)
            : base(message) { }
    }
}
