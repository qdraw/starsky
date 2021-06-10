using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeMetaPreflight : IMetaPreflight
	{

		public Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string, 
			List<string>> changedFileIndexItemName)> Preflight(FileIndexItem inputModel, 
			string[] inputFilePaths, bool append,
			bool collections, int rotateClock)
		{
			var detailView = new DetailView {FileIndexItem = inputModel};
			var changedFileIndexItemName = new Dictionary<string, List<string>>();

			if ( inputModel == null)
			{
				return Task.FromResult((new List<FileIndexItem>(),changedFileIndexItemName));
			}
			CompareAllLabelsAndRotation(changedFileIndexItemName,
				detailView,
				inputModel, append, rotateClock);

			if ( inputModel.FilePath == "/_database_changed_afterwards.jpg" )
			{
				// include updated changedFileIndexItemName
				return Task.FromResult( (new List<FileIndexItem>{new FileIndexItem()}, changedFileIndexItemName));
			}
			return Task.FromResult( (new List<FileIndexItem>{inputModel}, changedFileIndexItemName));
		}

		public void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName, DetailView detailView,
			FileIndexItem inputModel, bool append, int rotateClock)
		{
			if ( !changedFileIndexItemName.ContainsKey(detailView.FileIndexItem.FilePath) )
			{
				changedFileIndexItemName.Add(detailView.FileIndexItem.FilePath, new List<string>{"Tags"});
			}
		}

		public void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName,
			FileIndexItem collectionsFileIndexItem, FileIndexItem statusModel,
			bool append, int rotateClock)
		{
			throw new System.NotImplementedException();
		}
	}
}
