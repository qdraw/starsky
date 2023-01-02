using System.Collections.Generic;
using System.Diagnostics.Tracing;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.worker.CpuEventListener;

[Service(InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class CpuUsageListener : EventListener
{
	private readonly IWebLogger _logger;

	public CpuUsageListener(IWebLogger logger)	
	{
		_logger = logger;
	}
	public double LastValue { get; private set; }

	protected override void OnEventSourceCreated(EventSource eventSource)
	{
		if (eventSource.Name.Equals("System.Runtime"))
			EnableEvents(eventSource, EventLevel.LogAlways, 
				EventKeywords.All, 
				new Dictionary<string, string> { { "EventCounterIntervalSec", "15" } }!);
	}

	protected override void OnEventWritten(EventWrittenEventArgs eventData)
	{
		if (eventData.EventName != "EventCounters")
			return;

		if (eventData.Payload == null || eventData.Payload.Count == 0)
			return;
		if ( eventData.Payload[0] is not IDictionary<string, object>
			     eventPayload ||
		     !eventPayload.TryGetValue("Name", out var nameData) ||
		     nameData is not ("cpu-usage") ) return;
		
		if ( !eventPayload.TryGetValue("Mean", out var value) ) return;
		
		if ( value is not double dValue ) return;
		LastValue = dValue;

		_logger.LogDebug($"CPU Usage: {dValue}%");
		
		base.OnEventWritten(eventData);
	}
}



