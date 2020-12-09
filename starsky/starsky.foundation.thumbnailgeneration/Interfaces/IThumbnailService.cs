namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailService
	{
		bool CreateThumb(string subPath);
		bool CreateThumb(string subPath, string fileHash);
	}
}
