using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage
{
	public class ThumbnailFileMoveAllSizes
	{
		private readonly IStorage _thumbnailStorage;

		public ThumbnailFileMoveAllSizes(IStorage thumbnailStorage)
		{
			_thumbnailStorage = thumbnailStorage;
		}

		public void FileMove(string oldFileHash, string newHashCode)
		{
			_thumbnailStorage.FileMove(
				ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.Large), 
				ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.Large));
			_thumbnailStorage.FileMove(
				ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.Small), 
				ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.Small));
			_thumbnailStorage.FileMove(
				ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.ExtraLarge), 
				ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.ExtraLarge));
			_thumbnailStorage.FileMove(
				ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.TinyMeta), 
				ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.TinyMeta));
		}
	}
}
