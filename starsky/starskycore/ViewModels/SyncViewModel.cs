using starsky.Models;
using starskycore.Models;

namespace starsky.ViewModels
{
	public class SyncViewModel
	{
		public string FilePath { get; set; }
		public FileIndexItem.ExifStatus Status { get; set; }
	}
}
