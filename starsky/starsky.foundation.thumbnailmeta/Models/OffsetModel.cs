namespace starsky.foundation.readmeta.Models
{
	public class OffsetModel
	{
		/// <summary>
		/// Offset
		/// </summary>
		public int Index { get; set; }
		/// <summary>
		/// Size or count
		/// </summary>
		public int Count { get; set; }
		public bool Success { get; set; }
		public string Reason { get; set; }
		public byte[] Data { get; set; }
		public int SourceHeight { get; set; }
		public int SourceWidth { get; set; }
	}
}
