using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.CpuEventListener.Interfaces;

namespace starsky.foundation.worker.CpuEventListener;

[Service(typeof(ICpuUsageListener), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class CpuUsageListener : EventListener, ICpuUsageListener
{
	private readonly IWebLogger _logger;

	public CpuUsageListener(IWebLogger logger)	
	{
		_logger = logger;
	}
	public double CpuUsageMean { get; private set; }
	public bool IsReady { get; private set; } = false;

	protected override void OnEventSourceCreated(EventSource eventSource)
	{
		if (eventSource.Name.Equals("System.Runtime"))
			EnableEvents(eventSource, EventLevel.LogAlways, 
				EventKeywords.All, 
				new Dictionary<string, string> { { "EventCounterIntervalSec", "15" } }!);
	}

	protected override void OnEventWritten(EventWrittenEventArgs eventData)
	{
		UpdateEventData(eventData.EventName, eventData.Payload);
		base.OnEventWritten(eventData);
	}

	internal void UpdateEventData(string? eventDataEventName, ReadOnlyCollection<object?>? eventDataPayload)
	{
		if (eventDataEventName != "EventCounters" || eventDataPayload == null || eventDataPayload.Count == 0)
			return;

		if ( eventDataPayload[0] is not IDictionary<string, object>
			     eventPayload ||
		     !eventPayload.TryGetValue("Name", out var nameData) ||
		     nameData is not ("cpu-usage") ) return;
		
		if ( !eventPayload.TryGetValue("Mean", out var value) ) return;
		
		if ( value is not double dValue ) return;
		CpuUsageMean = dValue;
		IsReady = true;

		_logger.LogDebug($"CPU Usage: {dValue}%");
	}
}



