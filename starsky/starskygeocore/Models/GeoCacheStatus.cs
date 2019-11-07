namespace starskygeocore.Models
{
	public enum StatusType
	{
		Total,
		Current
	}
	
	public class GeoCacheStatus
	{
		public int Total { get; set; }
		public int Current { get; set; }
	}
}
