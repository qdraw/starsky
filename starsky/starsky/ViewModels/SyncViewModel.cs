using starsky.Models;

namespace starsky.ViewModels
{
	public class SyncViewModel
	{
		public string FilePath { get; set; }
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
