using System;

namespace starsky.foundation.realtime.Model
{
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

