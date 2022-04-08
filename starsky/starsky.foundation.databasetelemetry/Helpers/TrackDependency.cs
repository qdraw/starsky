#nullable enable
using System;
using System.Data.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public class TrackDependency
	{
		private readonly TelemetryClient? _telemetryClient;

		public TrackDependency(TelemetryClient? telemetryClient)
		{
			_telemetryClient = telemetryClient;
		}
		
		public bool Track(DbCommand command, DateTimeOffset? startTime, string name, string telemetryType, bool success = true)
		{
			if ( startTime == null ) return false;
			var duration = TimeSpan.Zero;
			if (startTime.Value != default)
			{
				duration = DateTimeOffset.UtcNow - startTime.Value;
			}
			
			var commandName = command.CommandText;
			_telemetryClient?.TrackDependency(new DependencyTelemetry()
			{
				Name = name,
				Data = commandName,
				Type = telemetryType,
				Duration = duration,
				Timestamp = startTime.Value,
				Success = success
			});
			
			return _telemetryClient != null;
		}

	}
}
