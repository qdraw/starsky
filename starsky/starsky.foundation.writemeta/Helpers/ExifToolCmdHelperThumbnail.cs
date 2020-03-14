using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starskycore.Models;

namespace starskycore.Helpers
{
	public partial class ExifToolCmdHelper
	{
		
		/// <summary>
		/// To update only the thumbnail hash
		/// </summary>
		/// <param name="updateModel"></param>
		/// <param name="comparedNames"></param>
		/// <returns></returns>
		public string UpdateThumbnail(FileIndexItem updateModel, List<string> comparedNames)
		{
			return UpdateAsyncWrapperThumbnail(updateModel, comparedNames).Result;
		}
		
		private async Task<string> UpdateAsyncWrapperThumbnail(FileIndexItem updateModel, List<string> comparedNames)
		{
			var task = Task.Run(() => UpdateASyncThumbnail(updateModel,comparedNames));
			return task.Wait(TimeSpan.FromSeconds(20)) ? task.Result : string.Empty;
		}
		
			    
		private async Task<string> UpdateASyncThumbnail(FileIndexItem updateModel, List<string> comparedNames)
		{
			var command = ExifToolCommandLineArgs(updateModel, comparedNames);
			await _exifTool.WriteTagsThumbnailAsync(updateModel.FileHash, command);
			return command;
		}
	}
}
