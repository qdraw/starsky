using System.Text;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.feature.webhtmlpublish.Helpers
{
	public class ToCreateSubfolder
	{
		private  readonly IStorage _hostFileSystemStorage;

		public ToCreateSubfolder(IStorage hostFileSystemStorage)
		{
			_hostFileSystemStorage = hostFileSystemStorage;
		}
		
		/// <summary>
		/// Create SubFolders by the profile.Folder setting
		/// </summary>
		/// <param name="profile">config</param>
		/// <param name="parentFolder">root folder</param>
		public void Create(AppSettingsPublishProfiles profile, string parentFolder)
		{
			// check if subfolder '1000' exist on disk
			// used for moving subfolders first
			var profileFolderStringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(parentFolder))
			{
				profileFolderStringBuilder.Append(parentFolder);
				profileFolderStringBuilder.Append("/");
			}
	        
			profileFolderStringBuilder.Append(profile.Folder);

			if ( _hostFileSystemStorage.IsFolderOrFile(profileFolderStringBuilder.ToString()) 
			     == FolderOrFileModel.FolderOrFileTypeList.Deleted)
			{
				_hostFileSystemStorage.CreateDirectory(profileFolderStringBuilder.ToString());
			}
		}
	}
}
