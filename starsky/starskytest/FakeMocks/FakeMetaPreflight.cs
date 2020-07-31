using System.Collections.Generic;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks
{
	public class FakeMetaPreflight : IMetaPreflight
	{

		public (List<FileIndexItem> fileIndexResultsList, Dictionary<string, List<string>>
			changedFileIndexItemName)
			Preflight(FileIndexItem inputModel,
				string[] inputFilePaths, bool append, bool collections, int rotateClock)
		{
			var detailView = new DetailView {FileIndexItem = inputModel};
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			CompareAllLabelsAndRotation(changedFileIndexItemName,
				detailView,
				inputModel, append, rotateClock);
			
			return ( new List<FileIndexItem>{inputModel}, changedFileIndexItemName);
		}

		public void CompareAllLabelsAndRotation(Dictionary<string, List<string>> changedFileIndexItemName, DetailView detailView,
			FileIndexItem inputModel, bool append, int rotateClock)
		{
			if ( !changedFileIndexItemName.ContainsKey(detailView.FileIndexItem.FilePath) )
			{
				changedFileIndexItemName.Add(detailView.FileIndexItem.FilePath, new List<string>{"Tags"});
			}
		}
	}
}
