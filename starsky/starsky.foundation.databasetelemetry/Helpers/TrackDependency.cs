using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public class TrackDependency
	{
		private readonly TelemetryClient _telemetryClient;

		public TrackDependency(TelemetryClient telemetryClient)
		{
			_telemetryClient = telemetryClient;
		}
		
		public bool Track(DbCommand command, DateTimeOffset startTime, string name, string telemetryType, bool success = true)
		{
			var duration = TimeSpan.Zero;
			if (startTime != default(DateTimeOffset))
			{
				duration = DateTimeOffset.UtcNow - startTime;
			}
			
			var commandName = command.CommandText;
			_telemetryClient.TrackDependency(new DependencyTelemetry()
			{
				Name = name,
				Data = commandName,
				Type = telemetryType,
				Duration = duration,
				Timestamp = startTime,
				Success = success
			});
			return true;
		}

	}
}
