using System;
using System.Diagnostics.CodeAnalysis;

namespace starsky.foundation.realtime.Model
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class HeartbeatModel
	{
		public HeartbeatModel(int speedInSeconds)
		{
			SpeedInSeconds = speedInSeconds;
		}
		public int SpeedInSeconds { get; set; }
		
		public DateTime DateTime { get; set; } = DateTime.UtcNow;
	}
}

