using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace starsky.foundation.realtime.Model
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class HeartbeatModel
	{
		public HeartbeatModel(int? speedInSeconds)
		{
			SpeedInSeconds = speedInSeconds;
		}
		
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? SpeedInSeconds { get; set; }
		
		public DateTime DateTime { get; set; } = DateTime.UtcNow;
	}
}

