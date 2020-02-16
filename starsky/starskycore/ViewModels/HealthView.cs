using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace starskycore.ViewModels
{
	public class HealthView
	{
		public bool IsHealthy { get; set; } = false;
		public List<HealthEntry> Entries { get; set; } = new List<HealthEntry>();
		
		public TimeSpan TotalDuration { get; set; }
	}
	
	public class HealthEntry {
		public string Name { get; set; }
		public TimeSpan Duration { get; set; }
		public bool IsHealthy { get; set; } = false;
	}
}
