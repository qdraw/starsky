using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeMetaPreflight : IMetaPreflight
	{

		public Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string, 
			List<string>> changedFileIndexItemName)> PreflightAsync(FileIndexItem inputModel, 
			List<string> inputFilePaths, bool append,
			bool collections, int rotateClock)
		{
						
			if ( inputModel != null && (string.IsNullOrEmpty(inputModel.FilePath) || inputModel.FilePath == "/") && inputFilePaths.Any() )
			{
				inputModel.FilePath = inputFilePaths.FirstOrDefault();
			}
			
			var detailView = new DetailView {FileIndexItem = inputModel};
			var changedFileIndexItemName = new Dictionary<string, List<string>>();

			if ( inputModel == null)
			{
				return Task.FromResult((new List<FileIndexItem>(),changedFileIndexItemName));
			}
			
			CompareAllLabelsAndRotation(changedFileIndexItemName,
				detailView,
				inputModel, append, rotateClock);

			if ( inputModel.Status == FileIndexItem.ExifStatus.Default )
			{
				inputModel.Status = FileIndexItem.ExifStatus.Ok;
			}


			if ( inputModel.FilePath == "/_database_changed_afterwards.jpg" )
			{
				// include updated changedFileIndexItemName
				return Task.FromResult( (new List<FileIndexItem>{new FileIndexItem()}, changedFileIndexItemName));
			}
			return Task.FromResult( (new List<FileIndexItem>{inputModel}, changedFileIndexItemName));
		}

		private static void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName, DetailView detailView,
			FileIndexItem _, bool _1, int _2)
		{
			if (detailView.FileIndexItem?.FilePath != null && !changedFileIndexItemName.ContainsKey(detailView.FileIndexItem.FilePath) )
			{
				changedFileIndexItemName.Add(detailView.FileIndexItem!.FilePath, new List<string>{"Tags"});
			}
		}

	}
}
