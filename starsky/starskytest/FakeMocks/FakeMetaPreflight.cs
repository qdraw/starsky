using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;

namespace starskytest.FakeMocks;

public class FakeMetaPreflight : IMetaPreflight
{
	public Task<(List<FileIndexItem> fileIndexResultsList, Dictionary<string,
		List<string>> changedFileIndexItemName)> PreflightAsync(FileIndexItem? inputModel,
		List<string> inputFilePaths, bool append,
		bool collections, int rotateClock)
	{
		if ( inputModel != null &&
		     ( string.IsNullOrEmpty(inputModel.FilePath) || inputModel.FilePath == "/" ) &&
		     inputFilePaths.Count != 0 )
		{
			inputModel.FilePath = inputFilePaths.FirstOrDefault();
		}

		var detailView = new DetailView { FileIndexItem = inputModel };
		var changedFileIndexItemName = new Dictionary<string, List<string>>();

		if ( inputModel == null )
		{
			return Task.FromResult(( new List<FileIndexItem>(), changedFileIndexItemName ));
		}

		CompareAllLabelsAndRotation(changedFileIndexItemName,
			detailView);

		if ( inputModel.Status == FileIndexItem.ExifStatus.Default )
		{
			inputModel.Status = FileIndexItem.ExifStatus.Ok;
		}


		if ( inputModel.FilePath == "/_database_changed_afterwards.jpg" )
		{
			// include updated changedFileIndexItemName
			return Task.FromResult(( new List<FileIndexItem> { new() }, changedFileIndexItemName ));
		}

		return Task.FromResult(( new List<FileIndexItem> { inputModel },
			changedFileIndexItemName ));
	}

	private static void CompareAllLabelsAndRotation(
		Dictionary<string, List<string>> changedFileIndexItemName, DetailView detailView)
	{
		if ( detailView.FileIndexItem?.FilePath != null &&
		     !changedFileIndexItemName.ContainsKey(detailView.FileIndexItem.FilePath) )
		{
			changedFileIndexItemName.Add(detailView.FileIndexItem!.FilePath,
				new List<string> { "Tags" });
		}
	}
}
