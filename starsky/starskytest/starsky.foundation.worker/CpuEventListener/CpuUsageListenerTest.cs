using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.worker.CpuEventListener;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.CpuEventListener
{
	[TestClass]
	public class CpuUsageListenerTests
	{
		[TestMethod]
		public void LastValue_Get_ReturnsExpectedValue()
		{
			// Arrange
			var listener = new CpuUsageListener();

			// Act
			var result = listener.CpuUsageMean;

			// Assert
			Assert.AreEqual(0.0, result);
		}

		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource()
		{
			// Arrange
			var listener = new CpuUsageListener();
			var eventSource = new EventSource("System.Runtime");

			// Get the OnEventSourceCreated method using reflection
			var method = typeof(CpuUsageListener).GetMethod("OnEventSourceCreated", BindingFlags.Instance | BindingFlags.NonPublic);

			// Act
			// LogAppContextSwitch
			method?.Invoke(listener, new object[] { eventSource });

			// Assert
			Assert.IsFalse(eventSource.IsEnabled());
		}
						
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_NoPayLoad2()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data = new List<object> { /* insert data here */ };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// no pay load
			
			// Assert
			Assert.IsFalse(listener.IsReady);
		}
		
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_WrongPayLoadData()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data2 = new Dictionary<string, object>
			{
				{"Name", "test"},
				{"Mean", "1"}
			};
			var data = new List<object> { data2 };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// wrong payload data
			
			// Assert
			Assert.IsFalse(listener.IsReady);
		}
		
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_WrongPayLoadData2()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data2 = new Dictionary<string, object>
			{
				{"Name", "cpu-usage"},
				{"Mean", "wrong-value"}
			};
			var data = new List<object> { data2 };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// wrong payload data
			
			// Assert
			Assert.IsFalse(listener.IsReady);
		}
		
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_WrongPayLoadData3()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data2 = new Dictionary<string, object>
			{
				{"Name", "cpu-usage"},
				{"No___Mean", "wrong-value"}
			};
			var data = new List<object> { data2 };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// wrong payload data
			
			// Assert
			Assert.IsFalse(listener.IsReady);
		}
		
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_NotDouble()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data2 = new Dictionary<string, object>
			{
				{"Name", "cpu-usage"},
				{"Mean", 1}
			};
			var data = new List<object> { data2 };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// wrong payload data
			
			// Assert
			Assert.IsFalse(listener.IsReady);
		}
		
				
		[TestMethod]
		public void OnEventSourceCreated_EnablesEventsForSystemRuntimeEventSource_HappyFlow()
		{
			// Arrange
			var listener = new CpuUsageListener();

			var data2 = new Dictionary<string, object>
			{
				{"Name", "cpu-usage"},
				{"Mean", 1d} // double
			};
			var data = new List<object> { data2 };
			var payload = new ReadOnlyCollection<object>(data);
			
			// Act
			listener.UpdateEventData("EventCounters", payload);
			// wrong payload data
			
			// Assert
			Assert.IsTrue(listener.IsReady);
		}
	}
}
